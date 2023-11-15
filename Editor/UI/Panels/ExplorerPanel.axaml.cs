using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Fliox.Editor.UI.Explorer;

// ReSharper disable ReplaceSliceWithRangeIndexer
// ReSharper disable UseIndexFromEndExpression
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable MergeIntoPattern
// ReSharper disable UnusedParameter.Local
namespace Friflo.Fliox.Editor.UI.Panels;

public partial class ExplorerPanel : UserControl, IEditorControl
{
    public  Editor          Editor      { get; private set; }

    
    public ExplorerPanel()
    {
        InitializeComponent();
        var viewModel           = new MainWindowViewModel();
        DataContext             = viewModel;
        DockPanel.ContextFlyout = new ExplorerFlyout(this);
        DragDrop.Focusable      = true;
        DragDrop.RowDrop       += RowDrop;
    }
    
    /// <summary>
    /// The only purpose of <see cref="RowDrop"/> and <see cref="RowDropped"/> is to select the
    /// <see cref="RowDropContext.droppedItems"/> after drag/drop is finished.
    /// </summary>
    private void RowDrop(object sender, TreeDataGridRowDragEventArgs args)
    {
        var cx = new RowDropContext();
        switch (args.Position)
        {
            case TreeDataGridRowDropPosition.Inside:
                cx.targetRow    = args.TargetRow;
                break;
            case TreeDataGridRowDropPosition.Before:
            case TreeDataGridRowDropPosition.After:
                var rows        = DragDrop.Rows!;
                var model       = rows.RowIndexToModelIndex(args.TargetRow.RowIndex);
                model           = model.Slice(0, model.Count - 1); // get parent IndexPath
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
        DragDrop.SelectItems(selection, indexes, SelectionView.First);
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
