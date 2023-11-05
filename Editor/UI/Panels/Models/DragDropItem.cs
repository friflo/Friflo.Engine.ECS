using System;
using System.Collections.ObjectModel;


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models
{
    // see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
    public class DragDropItem 
    {
        /// <summary> use either:
        /// <see cref="ObservableList{T}"/>       or
        /// <see cref="ObservableCollection{T}"/>
        /// </summary>
        public  readonly        ObservableList<DragDropItem>        children;   // Must always be present. Drop on an item with children == null result in COMException.
        public                  string                              Name { get; }
        public                  bool                                flag;

        private static          ObservableList<DragDropItem> CreateObservable() => new ObservableList<DragDropItem>();
        
        public  static readonly ObservableList<DragDropItem> Root = CreateRandomItems();
        
        private static          ObservableList<DragDropItem> CreateRandomItems()
        {
            var root = new DragDropItem ("root");
            root.children.Add(new DragDropItem("child 1"));
            root.children.Add(new DragDropItem("child 2"));
            root.children.Add(new DragDropItem("child 3"));
            root.children.Add(new DragDropItem("child 4"));
            var result = CreateObservable();
            result.Add(root);
            result.CollectionChanged += (sender, args) => {
                Console.WriteLine($"--- {args.Action}");
            };
            return result;
        }
        
        public DragDropItem(string name) {
            Name        = name;
            children    = CreateObservable();
            children.CollectionChanged += (sender, args) => {
                Console.WriteLine($"--- {args.Action}");
            };
        }
    }
}
