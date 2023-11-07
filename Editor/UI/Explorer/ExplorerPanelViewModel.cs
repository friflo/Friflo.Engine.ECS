using Friflo.Fliox.Editor.UI.Explorer.Lab;

namespace Friflo.Fliox.Editor.UI.Explorer;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/MainWindowViewModel.cs
internal class ExplorerPanelViewModel
{
    private DragDropPageViewModel   dragDrop;

    internal DragDropPageViewModel  DragDrop => dragDrop ??= new DragDropPageViewModel();
}

