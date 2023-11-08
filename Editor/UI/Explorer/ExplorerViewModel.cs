using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Friflo.Fliox.Editor.UI.Explorer.Lab;

namespace Friflo.Fliox.Editor.UI.Explorer;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/DragDropPageViewModel.cs
internal class ExplorerViewModel
{
    private static ITreeDataGridSource<ExplorerItem> CreateExplorerItemSource()
    {
        // var store       = ExplorerTree.TestStore;
        var store       = ExplorerTree.CreateDefaultStore();
        var tree        = new ExplorerTree(store);
        var rootItem    = tree.CreateExplorerItems(store);
        
        var source  = new HierarchicalTreeDataGridSource<ExplorerItem>(rootItem);
        
        var nameCol = new TextColumn<ExplorerItem, string>("name", item => item.Name, GridLength.Star);
        source.Columns.Add(new HierarchicalExpanderColumn<ExplorerItem>(nameCol, item => item));
        
        var idCol   = new TextColumn<ExplorerItem, int>   ("id",   item => item.Id,   GridLength.Auto);
        source.Columns.Add(idCol);
        
        var flagCol = new CheckBoxColumn<ExplorerItem>    ("flag", item => item.flag, (item, value) => item.flag = value);
        source.Columns.Add(flagCol);

        source.RowSelection!.SingleSelect = false;
        return source;
    }
    
    internal ITreeDataGridSource<ExplorerItem> ExplorerItemSource => CreateExplorerItemSource();
    
    // --------------------------------------- example data source ---------------------------------------
    /// Example showing the creation of a simple data source.
    private static ITreeDataGridSource<DragDropItem> CreateDragDropSource()
    {
        var data    = DragDropItem.Root;
        var source  = new HierarchicalTreeDataGridSource<DragDropItem>(data);
        
        source.Columns.Add(new HierarchicalExpanderColumn<DragDropItem>(
            new TextColumn<DragDropItem, string>("Name", x => x.Name, GridLength.Star), x => x.children));
        
        source.Columns.Add(
            new CheckBoxColumn<DragDropItem>("Flag", item => item.flag, (item, value) => item.flag = value));

        source.RowSelection!.SingleSelect = false;
        return source;
    }

    internal ITreeDataGridSource<DragDropItem> DragDropSource => CreateDragDropSource();
}

