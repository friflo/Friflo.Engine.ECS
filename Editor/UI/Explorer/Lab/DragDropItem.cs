using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Friflo.Fliox.Editor.UI.Explorer.Lab;

public class MyObservableCollection<T> : ObservableCollection<T>
{
    public void AddPropertyChangedHandler() {
        PropertyChanged += (_, args) => {
            var sb = new StringBuilder();
            sb.Append("PropertyChanged: ");
            var name = args.PropertyName;
            sb.Append(name);
            sb.Append(" = ");
            switch (name) {
                case "Count":   sb.Append(Count); break;
                case "Item[]":  sb.Append(Count); break;
            }
            Console.WriteLine(sb.ToString());
        }; 
    }
}

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
public class DragDropItem 
{
    /// <summary> use either:
    /// <see cref="ObservableList{T}"/>
    /// <see cref="ObservableCollection{T}"/>
    /// <see cref="MyObservableCollection{T}"/>
    /// </summary>
    public  readonly        MyObservableCollection<DragDropItem>        children;   // Must always be present. Drop on an item with children == null result in COMException.
    public                  string                              Name { get; }
    public                  bool                                flag;

    private static          MyObservableCollection<DragDropItem> CreateObservable() {
        var collection = new MyObservableCollection<DragDropItem>();
        collection.AddPropertyChangedHandler();
        return collection;
    }

    public  static readonly MyObservableCollection<DragDropItem> Root = CreateRandomItems();
    
    private static          MyObservableCollection<DragDropItem> CreateRandomItems()
    {
        var root = new DragDropItem ("root");
        root.children.Add(new DragDropItem("child 1"));
        root.children.Add(new DragDropItem("child 2"));
        root.children.Add(new DragDropItem("child 3"));
        root.children.Add(new DragDropItem("child 4"));
        var result = CreateObservable();
        result.Add(root);
        result.CollectionChanged += (_, args) => {
            Console.WriteLine($"--- {args.Action}");
        };
        return result;
    }
    
    public DragDropItem(string name) {
        Name        = name;
        children    = CreateObservable();
        children.CollectionChanged += (_, args) => {
            Console.WriteLine($"--- {args.Action}");
        };
    }
}

