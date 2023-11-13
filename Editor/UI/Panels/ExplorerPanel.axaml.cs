using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Fliox.Editor.UI.Explorer;

// ReSharper disable MergeIntoPattern
// ReSharper disable UnusedParameter.Local
namespace Friflo.Fliox.Editor.UI.Panels;

public partial class ExplorerPanel : UserControl, IEditorControl
{
    public Editor Editor { get; private set; }
    
    public ExplorerPanel()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        DockPanel.ContextFlyout = new ExplorerFlyout(this);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Editor = this.GetEditor(SetupExplorer);
    }
    
    /// <summary>
    /// Set <see cref="HierarchicalTreeDataGridSource{TModel}.Items"/> of <see cref="ExplorerViewModel.ExplorerItemSource"/>
    /// </summary>
    private void SetupExplorer()
    {
        if (Editor.Store == null) throw new InvalidOperationException("expect Store is present");
        // return;
        var source      = (HierarchicalTreeDataGridSource<ExplorerItem>)DragDrop.Source!;
        var store       = Editor.Store;
        var tree        = new ExplorerTree(store);
        var rootItem    = tree.CreateExplorerItems(store);
        source.Items    = new []{ rootItem };
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
