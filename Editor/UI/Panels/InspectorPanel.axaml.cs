using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Panels;

public partial class TestPanel : UserControl, IEditorControl
{
    public Editor Editor { get; private set; }
    
    public TestPanel()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Editor = this.GetEditor();
    }

    public void OnButtonClick(object sender, RoutedEventArgs routedEventArgs)
    {
        Console.WriteLine("Click");
    }
}