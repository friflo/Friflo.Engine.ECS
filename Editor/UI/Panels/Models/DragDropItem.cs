using System;
using System.Collections.ObjectModel;
using System.Linq;


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models
{
    // see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
    public class DragDropItem 
    {
        private static  Random                              _random = new Random(0);
        private         ObservableCollection<DragDropItem>? children;


        public DragDropItem(string name) => Name = name;
        public string   Name { get; }

        public bool     flag;

        public ObservableCollection<DragDropItem> Children => children ??= CreateRandomItems();

        public static ObservableCollection<DragDropItem> CreateRandomItems()
        {
            // return new ObservableCollection<DragDropItem>();
            
            var names = new Bogus.DataSets.Name();
            var count = _random.Next(10);
            return new ObservableCollection<DragDropItem>(Enumerable.Range(0, count)
                .Select(x => new DragDropItem(names.FullName())));
        }
    }
}
