using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Fliox.Editor.UI.Explorer;

// ReSharper disable UseIndexFromEndExpression
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable MergeIntoPattern
// ReSharper disable UnusedParameter.Local
namespace Friflo.Fliox.Editor.UI.Panels;

public partial class ExplorerPanel : UserControl, IEditorControl
{
    public  Editor          Editor      { get; private set; }
    public  ExplorerItem    RootItem    => GetRootItem();
    
    public ExplorerPanel()
    {
        InitializeComponent();
        var viewModel           = new MainWindowViewModel();
        DataContext             = viewModel;
        DockPanel.ContextFlyout = new ExplorerFlyout(this);
        DragDrop.Focusable      = true;
        DragDrop.RowDrop       += RowDrop;
    }
    
    private void RowDrop(object sender, TreeDataGridRowDragEventArgs args)
    {
        var rows    = DragDrop.Rows!;
        var cx      = new RowDropContext();
        switch (args.Position)
        {
            case TreeDataGridRowDropPosition.Inside:
                cx.targetRow    = args.TargetRow;
                break;
            case TreeDataGridRowDropPosition.Before:
            case TreeDataGridRowDropPosition.After:
                var model       = rows.RowIndexToModelIndex(args.TargetRow.RowIndex);
                model           = model.Slice(0, model.Count - 1);
                var rowIndex    = rows.ModelIndexToRowIndex(model);
                cx.targetRow    = DragDrop.TryGetRow(rowIndex);
                break;
            default:
                throw new InvalidOperationException("unexpected");
        }
        var items           = DragDrop.RowSelection!.SelectedItems;
        var droppedItems    = cx.droppedItems = new ExplorerItem[items.Count];
        for (int n = 0; n < items.Count; n++) {
            droppedItems[n] = (ExplorerItem)items[n];
        }
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
        var targetModel = DragDrop.Rows!.RowIndexToModelIndex(cx.targetRow.RowIndex);
        var selection   = MoveSelection.Create(targetModel, indexes);
        SelectItems(selection, indexes, SelectionView.First);
    }
    
    internal void FocusPanel()
    {
        DragDrop.Focus();
    }

    private ExplorerItem GetRootItem() {
        var source = (HierarchicalTreeDataGridSource<ExplorerItem>)DragDrop.Source!;
        return source.Items.First();
    }
    
    internal ExplorerItem[] GetSelectedItems() {
        var items = (IReadOnlyList<ExplorerItem>)DragDrop.RowSelection!.SelectedItems;
        return items.ToArray();
    }
    
    internal bool GetMoveSelection(out MoveSelection moveSelection) {
        var indexes = DragDrop.RowSelection!.SelectedIndexes;
        moveSelection = MoveSelection.Create(indexes);
        return moveSelection != null;
    }
    
    internal void SelectItems(MoveSelection moveSelection, int[] indexes, SelectionView view)
    {
        var parent      = moveSelection.parent;
        var selection   = DragDrop.RowSelection!;
        selection.BeginBatchUpdate();
        foreach (var index in indexes) {
            var child = parent.Append(index);
            selection.Select(child);
        }
        selection.EndBatchUpdate();
        
        BringSelectionIntoView(moveSelection.indexes, view);
    }
    
    private void BringSelectionIntoView(IReadOnlyList<IndexPath> indexes, SelectionView view) {
        var rows            = DragDrop.Rows!;
        var rowPresenter    = DragDrop.RowsPresenter!;
        if (view == SelectionView.First) {
            var firstIndex = rows.ModelIndexToRowIndex(indexes[0]);
            rowPresenter.BringIntoView(firstIndex - 1);
            return;            
        }
        var lastIndex = rows.ModelIndexToRowIndex(indexes[indexes.Count - 1]);
        rowPresenter.BringIntoView(lastIndex + 1);
    }
    
    private bool HandleKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Delete:
                ExplorerCommands.RemoveItems(GetSelectedItems(), RootItem, this);
                return true;
            case Key.N:
                if (e.KeyModifiers == KeyModifiers.Control) {
                    ExplorerCommands.CreateItems(GetSelectedItems(), this);
                    return true;
                }
                return false;
            case Key.Up:
                if (e.KeyModifiers == KeyModifiers.Control) {
                    if (GetMoveSelection(out var moveSelection)) {
                        var indexes = ExplorerCommands.MoveItemsUp(GetSelectedItems(), 1, this);
                        SelectItems(moveSelection, indexes, SelectionView.First);
                    }
                    return true;
                }
                return false;
            case Key.Down:
                if (e.KeyModifiers == KeyModifiers.Control) {
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
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        e.Handled = HandleKeyDown(e);
        base.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Editor = this.GetEditor(SetupExplorer);
    }
    
    /// <summary>
    /// Set <see cref="HierarchicalTreeDataGridSource{TModel}.Items"/> of <see cref="ExplorerViewModel.ExplorerItemSource"/>
    /// </summary>
    private void SetupExplorer()
    {
        if (Editor.Store == null) throw new InvalidOperationException("expect Store is present");
        // return;
        var source      = (HierarchicalTreeDataGridSource<ExplorerItem>)DragDrop.Source!;
        var rootEntity  = Editor.Store.StoreRoot;
        var tree        = new ExplorerTree(rootEntity);
        source.Items    = new []{ tree.rootItem };
    }

    private void DragDrop_OnRowDragStarted(object sender, TreeDataGridRowDragStartedEventArgs e)
    {
        foreach (ExplorerItem item in e.Models)
        {
            if (!item.AllowDrag) {
                e.AllowedEffects = DragDropEffects.None;
            }
        }
    }

    private void DragDrop_OnRowDragOver(object sender, TreeDataGridRowDragEventArgs e)
    {
        // Console.WriteLine($"OnRowDragOver: {e.Position} {e.TargetRow.Model}");
        if (e.TargetRow.Model is ExplorerItem explorerItem)
        {
            if (!explorerItem.IsRoot) {
                return;
            }
            if (e.Position == TreeDataGridRowDropPosition.Inside) {
                return;
            }
        }
        e.Inner.DragEffects = DragDropEffects.None;
    }
}
