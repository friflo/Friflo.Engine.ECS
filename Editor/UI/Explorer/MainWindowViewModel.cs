namespace Friflo.Fliox.Editor.UI.Explorer;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/MainWindowViewModel.cs
internal class MainWindowViewModel
{
    private ExplorerViewModel   dragDrop;

    internal ExplorerViewModel  DragDrop => dragDrop ??= new ExplorerViewModel();
}

