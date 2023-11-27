// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable UnusedParameter.Local
namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class GroupAdd : UserControl
{
    public  Entity  Entity      { get; set; }
    private string  GroupName   { get; set; }
    
    public GroupAdd()
    {
        InitializeComponent();
        List.SelectionChanged += (sender, args) => {
            var index = List.SelectedIndex;
            Console.WriteLine($"Select: {index}");
            Add(index);
            var button = EditorUtils.FindAncestor<Button>(this);
            button.Flyout?.Hide();
        };
    }

    internal void AddSchemaTypes(string groupName)
    {
        GroupName = groupName;
        var schema = EntityStore.GetEntitySchema();
        switch (groupName)
        {
            case "tags":
                var tags = schema.Tags;
                for (int n = 1; n < tags.Length; n++) {
                    var tag = tags[n];
                    List.Items.Add(new ListBoxItem { Content = tag.tagName }); 
                }
                break;
            case "components":
                var components = schema.Components;
                for (int n = 1; n < components.Length; n++) {
                    var component = components[n];
                    List.Items.Add(new ListBoxItem { Content = component.type.Name }); 
                }
                break;
            case "scripts":
                var scripts = schema.Scripts;
                for (int n = 1; n < scripts.Length; n++) {
                    var script = scripts[n];
                    List.Items.Add(new ListBoxItem { Content = script.type.Name }); 
                }
                break;
        }
    }
    
    private void Add(int index)
    {
        var schema = EntityStore.GetEntitySchema();
        index++; // Schema types start with index 1 
        switch (GroupName)
        {
            case "tags":
                var tagType = schema.Tags[index];
                var tag = new Tags(tagType);
                Entity.AddTags(tag);
                break;
            case "components":
                var componentType = schema.Components[index];
                Entity.AddEntityComponent(Entity, componentType);
                break;
            case "scripts":
                var scriptType = schema.Scripts[index];
                Entity.AddEntityScript(Entity, scriptType);
                break;
        }
    }
}