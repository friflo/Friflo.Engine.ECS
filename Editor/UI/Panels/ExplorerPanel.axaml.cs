using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Fliox.Editor.UI.Models;


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI;

public partial class ExplorerPanel : UserControl, IEditorControl
{
    public Editor Editor { get; private set; }
    
    public ExplorerPanel()
    {
        InitializeComponent();
        DataContext = new ExplorerPanelViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Editor = this.GetEditor(SetupExplorer);
    }
    
    private void SetupExplorer()
    {
        if (Editor.Store == null) throw new InvalidOperationException("expect Store is present");

    }
    
    private void DragDrop_RowDragStarted(object? sender, TreeDataGridRowDragStartedEventArgs e)
    {
        foreach (DragDropItem i in e.Models)
        {
            if (!i.AllowDrag)
                e.AllowedEffects = DragDropEffects.None;
        }
    }

    private void DragDrop_RowDragOver(object? sender, TreeDataGridRowDragEventArgs e)
    {
        if (e.Position == TreeDataGridRowDropPosition.Inside &&
            e.TargetRow.Model is DragDropItem i &&
            !i.AllowDrop)
            e.Inner.DragEffects = DragDropEffects.None;
    }
}
