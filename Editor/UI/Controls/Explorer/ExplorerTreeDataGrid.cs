using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.VisualTree;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable ReplaceSliceWithRangeIndexer
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable UseIndexFromEndExpression
namespace Friflo.Fliox.Editor.UI.Controls.Explorer;

/// <summary>
/// Extended <see cref="TreeDataGrid"/> should not be necessary. But is needed to get <see cref="OnKeyDown"/> callbacks.<br/>
/// <br/>
/// On Windows it is sufficient to handle these events in <see cref="Panels.ExplorerPanel.OnKeyDown"/>
/// but on macOS this method is not called.
/// </summary>
public class ExplorerTreeDataGrid : TreeDataGrid
{
    public  ExplorerItem    RootItem    => GetRootItem();
       
    // https://stackoverflow.com/questions/71815213/how-can-i-show-my-own-control-in-avalonia
    protected override Type StyleKeyOverride => typeof(TreeDataGrid);
    
    public ExplorerTreeDataGrid() {
        Focusable   = true; // required to obtain focus when moving items with: Ctrl + Up / Down
        RowDrop    += RowDropHandler;
        GotFocus   += (_, args) => {
            // Workaround:  Focus TreeDataGridTextCell if navigating with up/down keys.
            //              Otherwise it parent TreeDataGridExpanderCell is focus and F2 (rename) does not work.
            if (args.Source is TreeDataGridExpanderCell expanderCell) {
                var textCell = FindControl<TreeDataGridTextCell>(expanderCell);
                textCell?.Focus();
            }
        };
    }
    
    private static Control FindControl<T>(Visual control) where T : Control
    {
        foreach (var child in control.GetVisualChildren()) {
            if (child is not Control childControl) {
                continue;
            }
            if (childControl is T) {
                return childControl;
            }
            var sub = FindControl<T>(childControl);
            if (sub != null) {
                return sub;
            }
        }
        return null;
    }
    
    internal void FocusPanel() {
        Focus();
    }
    
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
        EditorUtils.Post(() => RowDropped(cx));
    }
    
    private void RowDropped(RowDropContext cx)
    {
        var indexes     = new int[cx.droppedItems.Length];
        int n           = 0;
        foreach (var item in cx.droppedItems) {
            var entity      = item.Entity;
            indexes[n++]    = entity.Parent.GetChildIndex(entity.Id);
        }
        var targetModel = cx.targetModel;
        var selection   = MoveSelection.Create(targetModel, indexes);
        SelectItems(selection, indexes, SelectionView.First);
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
    internal ExplorerItem[] GetSelectedItems() {
        var items = (IReadOnlyList<ExplorerItem>)RowSelection!.SelectedItems;
        return items.ToArray();
    }
    
    internal bool GetMoveSelection(out MoveSelection moveSelection) {
        var indexes = RowSelection!.SelectedIndexes;
        moveSelection = MoveSelection.Create(indexes);
        return moveSelection != null;
    }
    
    internal void SelectItems(MoveSelection moveSelection, int[] indexes, SelectionView view)
    {
        if (indexes.Length == 0) {
            return;
        }
        var parent      = moveSelection.parent;
        var selection   = RowSelection!;
        selection.BeginBatchUpdate();
        foreach (var index in indexes) {
            var child = parent.Append(index);
            selection.Select(child);
        }
        selection.EndBatchUpdate();
        
        BringSelectionIntoView(moveSelection, view);
    }
    
    private void BringSelectionIntoView(MoveSelection moveSelection, SelectionView view) {
        var rows            = Rows!;
        var rowPresenter    = RowsPresenter!;
        if (view == SelectionView.First) {
            var firstIndex = rows.ModelIndexToRowIndex(moveSelection.first) - 2;
            rowPresenter.BringIntoView(firstIndex);
            return;            
        }
        var lastIndex = rows.ModelIndexToRowIndex(moveSelection.last) + 2;
        rowPresenter.BringIntoView(lastIndex);
    }
    
    private bool HandleKeyDown(KeyEventArgs e)
    {
        var ctrlKey = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        switch (e.Key)
        {
            case Key.Delete:
                ExplorerCommands.RemoveItems(GetSelectedItems(), RootItem, this);
                return true;
            case Key.N:
                if (e.KeyModifiers == ctrlKey) {
                    ExplorerCommands.CreateItems(GetSelectedItems(), this);
                    return true;
                }
                return false;
            case Key.Up:
                if (e.KeyModifiers == ctrlKey) {
                    if (GetMoveSelection(out var moveSelection)) {
                        var indexes = ExplorerCommands.MoveItemsUp(GetSelectedItems(), 1, this);
                        SelectItems(moveSelection, indexes, SelectionView.First);
                    }
                    return true;
                }
                return false;
            case Key.Down:
                if (e.KeyModifiers == ctrlKey) {
                    if (GetMoveSelection(out var moveSelection)) {
                        var indexes = ExplorerCommands.MoveItemsDown(GetSelectedItems(), 1, this);
                        SelectItems(moveSelection, indexes, SelectionView.Last);
                    }
                    return true;
                }
                return false;
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

    protected override void OnKeyDown(KeyEventArgs e) {
        // Console.WriteLine($"ExplorerTreeDataGrid - OnKeyDown: {e.Key} {e.KeyModifiers}");
        e.Handled = HandleKeyDown(e);
        base.OnKeyDown(e);
    }
}