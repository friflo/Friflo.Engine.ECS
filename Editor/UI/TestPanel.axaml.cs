using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI;

public partial class TestPanel : UserControl
{
    public TestPanel()
    {
        InitializeComponent();
    }
    
    public void OnButtonClick(object sender, RoutedEventArgs routedEventArgs)
    {
        Console.WriteLine("Click");
    }
}