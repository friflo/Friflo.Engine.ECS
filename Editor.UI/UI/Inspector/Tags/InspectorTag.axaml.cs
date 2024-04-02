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

public partial class InspectorTag : UserControl
{
    public static readonly StyledProperty<string>   TagNameProperty  = AP.Register<InspectorTag, string>(nameof(TagName), "Tag");
    
    public string           TagName     { get => GetValue(TagNameProperty);  set => SetValue(TagNameProperty, value); }
    public Entity           Entity      { get; set; }
    public Tags             EntityTag   { get; init; }
    
    public InspectorTag()
    {
        InitializeComponent();
    }

    private void MenuItem_RemoveTag(object sender, RoutedEventArgs e) {
        Entity.RemoveTags(EntityTag);
        Console.WriteLine("MenuItem_RemoveTag");
    }
}