using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Fliox.Editor.UI.Explorer;

namespace Friflo.Fliox.Editor.UI.Panels;

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
}
