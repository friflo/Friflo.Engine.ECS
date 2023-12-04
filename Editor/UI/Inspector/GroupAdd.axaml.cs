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
    public  Entity              Entity      { get; set; }
    private string              GroupName   { get; set; }
    
    private InspectorControl    inspector;
    
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

    internal void AddSchemaTypes(InspectorControl inspector, string groupName)
    {
        this.inspector  = inspector;
        GroupName       = groupName;
        var schema      = EntityStore.GetEntitySchema();
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
                    List.Items.Add(new ListBoxItem { Content = component.name }); 
                }
                break;
            case "scripts":
                var scripts = schema.Scripts;
                for (int n = 1; n < scripts.Length; n++) {
                    var script = scripts[n];
                    List.Items.Add(new ListBoxItem { Content = script.name }); 
                }
                break;
        }
    }
    
    private void Add(int index)
    {
        var schema = EntityStore.GetEntitySchema();
        index++; // Schema types start with index 1 
        var entity = Entity;
        switch (GroupName)
        {
            case "tags":
                var tagType = schema.Tags[index];
                var tag = new Tags(tagType);
                entity.AddTags(tag);
                break;
            case "components":
                var componentType = schema.Components[index];
                if (componentType.type == typeof(EntityName)) {
                    if (entity.TryGetComponent<EntityName>(out var name)) {
                        entity.AddComponent(name);
                    } else {
                        Entity.AddEntityComponent(entity, componentType);
                        entity.Name.value = $"entity - {entity.Id}";
                    }
                } else {
                    Entity.AddEntityComponent(entity, componentType);
                }
                inspector.Observer.FocusComponent(componentType);
                break;
            case "scripts":
                var scriptType = schema.Scripts[index];
                Entity.AddNewEntityScript(entity, scriptType);
                inspector.Observer.FocusComponent(scriptType);
                break;
        }
    }
}