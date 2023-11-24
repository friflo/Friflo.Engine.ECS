// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Interactivity;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Fliox.Editor.UI.Panels;

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
        Console.WriteLine("Click");
    }
}