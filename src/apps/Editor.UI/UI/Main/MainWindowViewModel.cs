// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Editor.UI.Explorer;

namespace Friflo.Editor.UI.Main;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/MainWindowViewModel.cs
internal class MainWindowViewModel
{
    private ExplorerViewModel   explorerModel;

    internal ExplorerViewModel  ExplorerModel => explorerModel ??= new ExplorerViewModel();
}

