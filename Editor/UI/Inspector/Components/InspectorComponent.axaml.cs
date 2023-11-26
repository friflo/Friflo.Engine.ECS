// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class InspectorComponent : UserControl, IExpandable
{
    public static readonly StyledProperty<string>   ComponentTitleProperty  = AP.Register<InspectorComponent, string>(nameof(ComponentTitle), "Component");
    public static readonly StyledProperty<bool>     ExpandedProperty        = AP.Register<InspectorComponent, bool>  (nameof(Expanded), true);
    
    public string   ComponentTitle  { get => GetValue(ComponentTitleProperty);  set => SetValue(ComponentTitleProperty, value); }
    public bool     Expanded        { get => GetValue(ExpandedProperty);        set => SetValue(ExpandedProperty,       value); }
    
    public InspectorComponent()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e) {
        Expanded = !Expanded;
    }

    private void MenuItem_RemoveComponent(object sender, RoutedEventArgs e) {
        Console.WriteLine("MenuItem_RemoveComponent");
    }
}