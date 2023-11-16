using Friflo.Fliox.Editor.UI.Explorer;

namespace Friflo.Fliox.Editor.UI.Main;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/MainWindowViewModel.cs
internal class MainWindowViewModel
{
    private ExplorerViewModel   explorerModel;

    internal ExplorerViewModel  ExplorerModel => explorerModel ??= new ExplorerViewModel();
}

