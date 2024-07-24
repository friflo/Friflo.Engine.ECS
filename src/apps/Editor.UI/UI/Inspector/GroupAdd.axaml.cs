// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Friflo.Editor.Utils;
using Friflo.Engine.ECS;

// ReSharper disable UnusedParameter.Local
namespace Friflo.Editor.UI.Inspector;

public partial class GroupAdd : UserControl
{
    public  Entity              Entity      { get; set; }
    private string              GroupName   { get; set; }
    
    private InspectorControl    inspector;
    
    public GroupAdd()
    {
        InitializeComponent();
        List.SelectionChanged += (sender, args) => {
            var item    = (ListBoxItem)List.SelectedItem;
            var key     = item!.Name;
            Console.WriteLine($"Select: {key}");
            Add(key);
            var button  = EditorUtils.FindAncestor<Button>(this);
            button.Flyout?.Hide();
        };
    }

    internal void AddSchemaTypes(InspectorControl inspector, string groupName)
    {
        this.inspector  = inspector;
        GroupName       = groupName;
        var schema      = EntityStore.GetEntitySchema();
        List.Items.Clear(); // clear example ListBoxItem's
        switch (groupName)
        {
            case "tags":
                var tags = schema.Tags;
                for (int n = 1; n < tags.Length; n++) {
                    var tag = tags[n];
                    List.Items.Add(new ListBoxItem { Content = tag.TagName, Name = tag.TagName });
                }
                break;
            case "components":
                var components = schema.Components;
                for (int n = 1; n < components.Length; n++) {
                    var component = components[n];
                    if (component.Type == typeof(Unresolved)) {
                        continue;
                    }
                    List.Items.Add(new ListBoxItem { Content = component.Name, Name = component.ComponentKey });
                }
                break;
            case "scripts":
                var scripts = schema.Scripts;
                for (int n = 1; n < scripts.Length; n++) {
                    var script = scripts[n];
                    List.Items.Add(new ListBoxItem { Content = script.Name, Name = script.ComponentKey });
                }
                break;
        }
    }
    
    private void Add(string key)
    {
        var schema = EntityStore.GetEntitySchema();
        var entity = Entity;
        switch (GroupName)
        {
            case "tags":
                var tagType = schema.TagTypeByName[key];
                var tag     = new Tags(tagType);
                entity.AddTags(tag);
                break;
            case "components":
                var componentType = (ComponentType)schema.SchemaTypeByKey[key];
                if (componentType.Type == typeof(EntityName)) {
                    if (entity.TryGetComponent<EntityName>(out var name)) {
                        entity.AddComponent(name);
                    } else {
                        EntityUtils.AddEntityComponent(entity, componentType);
                        entity.Name.value = $"entity - {entity.Id}";
                    }
                } else {
                    EntityUtils.AddEntityComponent(entity, componentType);
                }
                inspector.Observer.FocusComponent(componentType);
                break;
            case "scripts":
                var scriptType = (ScriptType)schema.SchemaTypeByKey[key];
                EntityUtils.AddNewEntityScript(entity, scriptType);
                inspector.Observer.FocusComponent(scriptType);
                break;
        }
    }
}