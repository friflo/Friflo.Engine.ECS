using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Friflo.Fliox.Editor.UI.Explorer.Lab;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/DragDropPageViewModel.cs
internal class DragDropPageViewModel
{
    internal DragDropPageViewModel()
    {
        var data    = DragDropItem.Root;
        var source  = new HierarchicalTreeDataGridSource<DragDropItem>(data);
        
        source.Columns.Add(new HierarchicalExpanderColumn<DragDropItem>(
            new TextColumn<DragDropItem, string>("Name", x => x.Name, GridLength.Star), x => x.children));
        
        source.Columns.Add(new CheckBoxColumn<DragDropItem>(
            "Flag", x => x.flag, (o, x) => o.flag = x));

        source.RowSelection!.SingleSelect = false;
        Source = source;
    }

    internal ITreeDataGridSource<DragDropItem> Source { get; }
}

