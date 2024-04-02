// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Friflo.Editor.UI.Panels;
using Friflo.Editor.Utils;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Collections;

// ReSharper disable UnusedParameter.Local
// ReSharper disable ReplaceSliceWithRangeIndexer
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable UseIndexFromEndExpression
namespace Friflo.Editor.UI.Explorer;

/// <summary>
/// Extended <see cref="TreeDataGrid"/> should not be necessary. But is needed to get <see cref="OnKeyDown"/> callbacks.<br/>
/// <br/>
/// On Windows it is sufficient to handle these events in <see cref="Panels.ExplorerPanel.OnKeyDown"/>
/// but on macOS this method is not called.
/// </summary>
public class ExplorerTreeDataGrid : TreeDataGrid
{
    public  ExplorerItem    RootItem    => GetRootItem();
    private AppEvents       appEvents;
       
    // https://stackoverflow.com/questions/71815213/how-can-i-show-my-own-control-in-avalonia
    protected override Type StyleKeyOverride => typeof(TreeDataGrid);
    
    public ExplorerTreeDataGrid() {
        Focusable   = true; // required to obtain focus when moving items with: Ctrl + Up / Down
        RowDrop    += RowDropHandler;
        // GotFocus   += (_, args) => FocusWorkaround(args);
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        appEvents = this.GetEditor();
        appEvents?.AddObserver(new ExplorerObserver(this, appEvents));
        
        // condition to view in Designer
        if (RowSelection != null) {
            RowSelection.SelectionChanged += (_, args) => OnSelectionChanged(args);
        }
    }
    
    // private readonly    HashSet<ExplorerItem>   selectedItems = new HashSet<ExplorerItem>();
    // private             ExplorerItem            selectedItem;
    
    private void OnSelectionChanged(TreeSelectionModelSelectionChangedEventArgs _)
    {
        var selectedItem = RowSelection!.SelectedItem as ExplorerItem;
        appEvents.SelectionChanged(new EditorSelection { item = selectedItem });
    }
    
    /*
    /// <summary>
    /// Workaround:  Focus TreeDataGridTextCell if navigating with up/down keys.
    ///              Otherwise if parent is <see cref="TreeDataGridExpanderCell"/> its focus and F2 (rename) does not work.
    /// </summary>
    private static void FocusWorkaround(GotFocusEventArgs args)
    {
        if (args.Source is TreeDataGridExpanderCell expanderCell) {
            var textCell = EditorUtils.FindControl<TreeDataGridTextCell>(expanderCell);
            // textCell?.Focus(); - calling Focus() explicit corrupt navigation with Key.Tab
        }
    } */
    
    /// <summary>
    /// The only purpose of <see cref="RowDropHandler"/> and <see cref="RowDropped"/> is to select the
    /// <see cref="RowDropContext.droppedItems"/> after drag/drop is finished.
    /// </summary>
    private void RowDropHandler(object sender, TreeDataGridRowDragEventArgs args)
    {
        var cx = new RowDropContext();
        switch (args.Position)
        {
            case TreeDataGridRowDropPosition.Inside:
                cx.targetModel  = Rows!.RowIndexToModelIndex(args.TargetRow.RowIndex);
                break;
            case TreeDataGridRowDropPosition.Before:
            case TreeDataGridRowDropPosition.After:
                var rows        = Rows!;
                var model       = rows.RowIndexToModelIndex(args.TargetRow.RowIndex);
                cx.targetModel  = model.Slice(0, model.Count - 1); // get parent IndexPath
                break;
            default:
                throw new InvalidOperationException("unexpected");
        }
        cx.droppedItems = GetSelectedItems();
        StoreDispatcher.Post(() => RowDropped(cx));
    }
    
    private void RowDropped(RowDropContext cx)
    {
        var indexes     = new int[cx.droppedItems.Length];
        int n           = 0;
        foreach (var item in cx.droppedItems) {
            var entity      = item.Entity;
            indexes[n++]    = entity.Parent.GetChildIndex(entity);
        }
        var targetModel     = cx.targetModel;
        var newSelection    = TreeIndexPaths.Create(targetModel, indexes);
        // newSelection.UpdateIndexPaths(indexes); todo fix
        SelectItems(newSelection, SelectionView.First, 1);
    }
    
    internal HierarchicalTreeDataGridSource<ExplorerItem> GridSource => (HierarchicalTreeDataGridSource<ExplorerItem>)Source!;
    
    private ExplorerItem GetRootItem() {
        return GridSource.Items.First();
    }
    
    /// <summary>
    /// <b>Note</b>: Seem an issue in Avalonia. In some case:
    /// <code>
    ///     RowSelection.SelectedItems.Count != RowSelection.SelectedItems.ToArray().Length
    /// </code>
    /// => use this method when accessing <see cref="ITreeSelectionModel.SelectedItems"/>
    /// </summary>
    private ExplorerItem[] GetSelectedItems() {
        var items = (IReadOnlyList<ExplorerItem>)RowSelection!.SelectedItems;
        // create a copy as SelectedItems may change
        return items.ToArray();
    }
    
    internal TreeSelection GetSelection() {
        var items = GetSelectedItems();
        return new TreeSelection(items);
    }
    
    internal bool GetSelectedPaths(out TreeIndexPaths selectedPaths) {
        var indexes     = RowSelection!.SelectedIndexes;
        selectedPaths   = TreeIndexPaths.Create(indexes);
        return selectedPaths != null;
    }
    
    internal void SelectItems(TreeIndexPaths treeIndexPaths, SelectionView view, int margin)
    {
        var paths       = treeIndexPaths.paths;
        var selection   = RowSelection!;
        selection.BeginBatchUpdate();
        IndexPath first = default;
        IndexPath last  = default;
        int length      = paths.Length;
        for (int n = 0; n < length; n++) {
            var path    = paths[n];
            if (n == 0) {
                first = path;
            }
            if (n == length - 1) {
                last = path;
            }
            selection.Select(path);
        }
        selection.EndBatchUpdate();
        
        BringSelectionIntoView (first, last, view, margin);
    }
    
    private void BringSelectionIntoView(IndexPath first, IndexPath last, SelectionView view, int margin) {
        var rows = Rows!;
        int showIndex;
        if (view == SelectionView.First) {
            showIndex = rows.ModelIndexToRowIndex(first) - margin;
        } else {
            showIndex = rows.ModelIndexToRowIndex(last)  + margin;
        }
        // Console.WriteLine($"BringIntoView: {showIndex}");
        RowsPresenter!.BringIntoView(showIndex);
    }
    
    private bool HandleKeyUp(KeyEventArgs e) {
        if (e.Key == Key.F2) {
            ExplorerCommands.RenameEntity(this);
            return true;
        }
        return false;
    }
    
    /// <remarks>
    /// <see cref="Key.F2"/> is handled by <see cref="TreeDataGrid"/> if focused.<br/>
    /// Otherwise it is handled at <see cref="HandleKeyUp"/>
    /// </remarks>
    private bool HandleKeyDown(KeyEventArgs e)
    {
        var ctrlKey = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        switch (e.Key)
        {
            case Key.C:
                if (e.KeyModifiers != ctrlKey) {
                    return false;
                }
                ExplorerCommands.CopyItems(GetSelection(), this);
                return true;
            case Key.V:
                if (e.KeyModifiers != ctrlKey) {
                    return false;
                }
                ExplorerCommands.PasteItems(GetSelection(), this);
                return true;
            case Key.D:
                if (e.KeyModifiers != ctrlKey) {
                    return false;
                }
                ExplorerCommands.DuplicateItems(GetSelection(), this);
                return true;
            case Key.Delete:
                ExplorerCommands.RemoveItems(GetSelection(), this);
                return true;
            case Key.N:
                if (e.KeyModifiers != ctrlKey) {
                    return false;
                }
                ExplorerCommands.CreateItems(GetSelection(), this);
                return true;
            case Key.Up:
                if (e.KeyModifiers != ctrlKey) {
                    return false;
                }
                if (GetSelectedPaths(out var selectedPaths)) {
                    var indexes = ExplorerCommands.MoveItemsUp(GetSelection(), 1, this);
                    selectedPaths.UpdateLeafIndexes(indexes);
                    SelectItems(selectedPaths, SelectionView.First, 1);
                }
                return true;
            case Key.Down:
                if (e.KeyModifiers != ctrlKey) {
                    return false;
                }
                if (GetSelectedPaths(out selectedPaths)) {
                    var indexes = ExplorerCommands.MoveItemsDown(GetSelection(), 1, this);
                    selectedPaths.UpdateLeafIndexes(indexes);
                    SelectItems(selectedPaths, SelectionView.Last, 1);
                }
                return true;
            default:
                return false;
        }
    }
    
    internal IndexPath GetSelectionPath()
    {
        return RowSelection!.SelectedIndexes[0];
    }
    
    internal void SetSelectionPath(IndexPath path)
    {
        var rowIndex = Rows!.ModelIndexToRowIndex(path);
        if (rowIndex == -1) {
            path = path.Slice(0, path.Count - 1);
        }
        RowSelection!.SelectedIndex = path;
    }
    
    private bool HandleGetFocus(GotFocusEventArgs e)
    {
        if (e.NavigationMethod == NavigationMethod.Tab) {
            bool passFocus = e.Source is TreeDataGrid; // || e.Source is TreeDataGridColumnHeader;
            if (e.KeyModifiers == KeyModifiers.None) {
                if (passFocus) {
                    return false;
                }
                var ancestor    = this.FindAncestorOfType<PanelControl>();
                var next        = EditorUtils.GetTabIndex(ancestor, +1);
                next.Focus(NavigationMethod.Tab);
                return true;
            }
            if (passFocus) {
                return false;
            }
            Focus(NavigationMethod.Tab, KeyModifiers.Shift);
            return true;
        }
        return false;
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        e.Handled = HandleGetFocus(e);
        base.OnGotFocus(e);
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        e.Handled = HandleKeyUp(e);
        base.OnKeyUp(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        // Console.WriteLine($"ExplorerTreeDataGrid - OnKeyDown: {e.Key} {e.KeyModifiers}");
        e.Handled = HandleKeyDown(e);
        base.OnKeyDown(e);
    }
}