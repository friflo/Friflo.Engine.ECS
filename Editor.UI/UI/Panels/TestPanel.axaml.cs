// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Avalonia;
using Avalonia.Interactivity;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Editor.UI.Panels;

public partial class TestPanel : PanelControl
{
    public static readonly StyledProperty<bool>   ExplorerProperty  = AP.Register<TestPanel, bool>(nameof(Explorer), true); // todo check exception using true

    public  bool  Explorer  { get => GetValue(ExplorerProperty);  set => SetValue(ExplorerProperty, value); }

    
    public TestPanel()
    {
        InitializeComponent();
    }

    public void OnButtonClick(object sender, RoutedEventArgs routedEventArgs)
    {
        ProcessStartInfo sInfo = new ProcessStartInfo("http://localhost:5000") { UseShellExecute = true };
#pragma warning disable RS0030
        Process.Start(sInfo);
#pragma warning restore RS0030
    }
}