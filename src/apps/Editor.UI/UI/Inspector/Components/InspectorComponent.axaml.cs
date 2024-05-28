// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Engine.ECS;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class InspectorComponent : UserControl, IExpandable
{
    public static readonly StyledProperty<string>   ComponentTitleProperty  = AP.Register<InspectorComponent, string>(nameof(ComponentTitle), "Component");
    public static readonly StyledProperty<bool>     ExpandedProperty        = AP.Register<InspectorComponent, bool>  (nameof(Expanded), true);
    
    public string   ComponentTitle  { get => GetValue(ComponentTitleProperty);  set => SetValue(ComponentTitleProperty, value); }
    public bool     Expanded        { get => GetValue(ExpandedProperty);        set => SetValue(ExpandedProperty,       value); }
    
    public Entity           Entity          { get; set; }
    public ComponentType    ComponentType   { get; init; }
    public ScriptType       ScriptType      { get; init; }

    public override string  ToString()      => $"Title: {ComponentTitle}, Expanded: {Expanded}, IsVisible: {IsVisible}";

    public InspectorComponent()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e) {
        Expanded = !Expanded;
    }

    private void MenuItem_RemoveComponent(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("MenuItem_RemoveComponent");
        if (ComponentType != null) {
            EntityUtils.RemoveEntityComponent(Entity, ComponentType);
        }
        if (ScriptType != null) {
            EntityUtils.RemoveEntityScript(Entity, ScriptType);
        }
    }
}