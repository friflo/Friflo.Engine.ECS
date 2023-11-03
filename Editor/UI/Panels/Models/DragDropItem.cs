using System.Collections.ObjectModel;


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models
{
    // see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
    public class DragDropItem 
    {
        public  readonly    ObservableCollection<DragDropItem>  children;   // Must always be present. Drop on an item with children == null result in COMException.
        public              string                              Name { get; }
        public              bool                                flag;

        public DragDropItem(string name) {
            Name        = name;
            children    = new ObservableCollection<DragDropItem>();
        }

        public static ObservableCollection<DragDropItem> CreateRandomItems()
        {
            var root = new DragDropItem ("root");
            root.children.Add(new DragDropItem("child 1"));
            root.children.Add(new DragDropItem("child 2"));
            return new ObservableCollection<DragDropItem> { root };
        }
    }
}
