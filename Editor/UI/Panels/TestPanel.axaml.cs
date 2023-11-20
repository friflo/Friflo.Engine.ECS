// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Panels;

public partial class TestPanel : UserControl
{
    public TestPanel()
    {
        InitializeComponent();
    }

    // ReSharper disable once RedundantOverriddenMember
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
    }

    public void OnButtonClick(object sender, RoutedEventArgs routedEventArgs)
    {
        Console.WriteLine("Click");
    }
    
    protected override void OnGotFocus(GotFocusEventArgs e) {
        base.OnGotFocus(e);
        EditorUtils.SetActivePanel(this);
    }
}