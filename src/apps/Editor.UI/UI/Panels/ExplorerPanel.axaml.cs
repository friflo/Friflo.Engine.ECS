// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Input;
using Friflo.Editor.UI.Explorer;
using Friflo.Editor.UI.Main;
using Friflo.Engine.ECS.Collections;

// ReSharper disable UnusedParameter.Local
namespace Friflo.Editor.UI.Panels;

public partial class ExplorerPanel : PanelControl
{
    public override string ToString() => Grid.RootItem.DebugTreeName;
    
    public ExplorerTreeDataGrid TreeDataGrid => Grid;

    public ExplorerPanel()
    {
        InitializeComponent();
        var viewModel           = new MainWindowViewModel();
        DataContext             = viewModel;
        DockPanel.ContextFlyout = new ExplorerFlyout(Grid);
    }

    private void DragDrop_OnRowDragStarted(object sender, TreeDataGridRowDragStartedEventArgs e)
    {
        foreach (ExplorerItem item in e.Models)
        {
            if (!item.AllowDrag) {
                e.AllowedEffects = DragDropEffects.None;
            }
        }
    }
    
    public override bool OnExecuteCommand(EditorCommand command)
    {
        switch (command) {
            case CopyToClipboardCommand:
                ExplorerCommands.CopyItems(Grid.GetSelection(), Grid);
                return true;
        }
        return false;
    }

    private void DragDrop_OnRowDragOver(object sender, TreeDataGridRowDragEventArgs e)
    {
        // Console.WriteLine($"OnRowDragOver: {e.Position} {e.TargetRow.Model}");
        if (e.TargetRow.Model is ExplorerItem explorerItem)
        {
            if (!explorerItem.IsRoot) {
                return;
            }
            if (e.Position == TreeDataGridRowDropPosition.Inside) {
                return;
            }
        }
        e.Inner.DragEffects = DragDropEffects.None;
    }
}
