using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Friflo.Fliox.Editor.UI.Explorer.Lab;

namespace Friflo.Fliox.Editor.UI.Explorer;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/DragDropPageViewModel.cs
internal class ExplorerViewModel
{
    /// Example showing the creation of a simple data source.
    private static ITreeDataGridSource<DragDropItem> CreateDragDropSource()
    {
        var data    = DragDropItem.Root;
        var source  = new HierarchicalTreeDataGridSource<DragDropItem>(data);
        
        source.Columns.Add(new HierarchicalExpanderColumn<DragDropItem>(
            new TextColumn<DragDropItem, string>("Name", x => x.Name, GridLength.Star), x => x.children));
        
        source.Columns.Add(new CheckBoxColumn<DragDropItem>(
            "Flag", x => x.flag, (o, x) => o.flag = x));

        source.RowSelection!.SingleSelect = false;
        return source;
    }

    internal ITreeDataGridSource<DragDropItem> DragDropSource => CreateDragDropSource();
}

