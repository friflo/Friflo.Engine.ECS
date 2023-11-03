using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Models
{
    // see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/Models/DragDropItem.cs
    public class DragDropItem : ReactiveObject
    {
        private static Random _random = new Random(0);
        private ObservableCollection<DragDropItem>? _children;
        private bool _allowDrag = true;
        private bool _allowDrop = true;

        public DragDropItem(string name) => Name = name;
        public string Name { get; }

        public bool AllowDrag
        {
            get => _allowDrag;
            set => this.RaiseAndSetIfChanged(ref _allowDrag, value);
        }

        public bool AllowDrop
        {
            get => _allowDrop;
            set => this.RaiseAndSetIfChanged(ref _allowDrop, value);
        }

        public ObservableCollection<DragDropItem> Children => _children ??= CreateRandomItems();

        public static ObservableCollection<DragDropItem> CreateRandomItems()
        {
            var names = new Bogus.DataSets.Name();
            var count = _random.Next(10);
            return new ObservableCollection<DragDropItem>(Enumerable.Range(0, count)
                .Select(x => new DragDropItem(names.FullName())));
        }
    }
}
