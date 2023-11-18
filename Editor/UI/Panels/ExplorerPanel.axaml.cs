using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Fliox.Editor.UI.Controls.Explorer;
using Friflo.Fliox.Editor.UI.Main;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable UnusedParameter.Local
namespace Friflo.Fliox.Editor.UI.Panels;

public partial class ExplorerPanel : UserControl
{
    public ExplorerPanel()
    {
        InitializeComponent();
        var viewModel           = new MainWindowViewModel();
        DataContext             = viewModel;
        DockPanel.ContextFlyout = new ExplorerFlyout(Grid);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        new ExplorerEditorObserver(this, this.GetEditor()).Register();
    }
    
    private class ExplorerEditorObserver : EditorObserver
    {
        private readonly ExplorerPanel panel;
        
        internal ExplorerEditorObserver (ExplorerPanel panel, Editor editor) : base (editor) { this.panel = panel; }
        
        /// <summary>
        /// Set <see cref="HierarchicalTreeDataGridSource{TModel}.Items"/> of <see cref="ExplorerViewModel.ExplorerItemSource"/>
        /// </summary>
        protected override void OnEditorReady()
        {
            var store       = Editor.Store;
            if (store == null) throw new InvalidOperationException("expect Store is present");
            // return;
            var source      = panel.Grid.GridSource;
            var rootEntity  = store.StoreRoot;
            var tree        = new ExplorerItemTree(rootEntity, $"tree-{_treeCount++}");
            source.Items    = new []{ tree.RootItem };
        }
    }
    
    private static      int _treeCount;

    private void DragDrop_OnRowDragStarted(object sender, TreeDataGridRowDragStartedEventArgs e)
    {
        foreach (ExplorerItem item in e.Models)
        {
            if (!item.AllowDrag) {
                e.AllowedEffects = DragDropEffects.None;
            }
        }
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
