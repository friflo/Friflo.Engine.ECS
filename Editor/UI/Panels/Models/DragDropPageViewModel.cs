using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models
{
    // see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/DragDropPageViewModel.cs
    internal class DragDropPageViewModel
    {
        private readonly ObservableCollection<DragDropItem> data;

        internal DragDropPageViewModel()
        {
            data = DragDropItem.CreateRandomItems();
            var source = new HierarchicalTreeDataGridSource<DragDropItem>(data)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<DragDropItem>(
                        new TextColumn<DragDropItem, string>(
                            "Name",
                            x => x.Name,
                            GridLength.Star),
                        x => x.Children),
                    new CheckBoxColumn<DragDropItem>(
                        "Allow Drag",
                        x => x.allowDrag,
                        (o, x) => o.allowDrag = x),
                    new CheckBoxColumn<DragDropItem>(
                        "Allow Drop",
                        x => x.allowDrop,
                        (o, x) => o.allowDrop = x),
                }
            };

            source.RowSelection!.SingleSelect = false;
            Source = source;
        }

        internal ITreeDataGridSource<DragDropItem> Source { get; }
    }
}
