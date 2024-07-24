// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Friflo.Engine.ECS;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class UnresolvedField : UserControl, IFieldControl
{
    public  ComponentField  ComponentField  { get; init; }
    
    
    internal void Set(Unresolved unresolved)
    {
        var tags            = unresolved.tags;
        var components      = unresolved.components;
        var tagItems        = Tags.Items;
        var componentItems  = Components.Items;
        tagItems.Clear();
        componentItems.Clear();

        if (tags != null) {
            foreach (var tag in tags) {
                tagItems.Add(new ListBoxItem { Content = tag });
            }
        }
        if (components != null) {
            foreach (var component in components) {
                componentItems.Add(new ListBoxItem { Content = component.key });
            }
        }
    }

    public UnresolvedField()
    {
        InitializeComponent();
    }
}