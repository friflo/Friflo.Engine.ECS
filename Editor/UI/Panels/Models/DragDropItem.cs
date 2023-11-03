using System.Collections.ObjectModel;


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models
{
    // see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
    public class DragDropItem 
    {
        public  ObservableCollection<DragDropItem>  children;
        public  string                              Name { get; }
        public  bool                                flag;

        public DragDropItem(string name) => Name = name;

        public static ObservableCollection<DragDropItem> CreateRandomItems()
        {
            var root = new DragDropItem ("root") {
                children = new ObservableCollection<DragDropItem> {
                    new DragDropItem("child 1"),
                    new DragDropItem("child 12")
                }
            };
            return new ObservableCollection<DragDropItem> { root };
        }
    }
}
