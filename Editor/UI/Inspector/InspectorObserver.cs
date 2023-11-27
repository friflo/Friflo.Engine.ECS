// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;

// ReSharper disable ParameterTypeCanBeEnumerable.Local
namespace Friflo.Fliox.Editor.UI.Inspector;


internal readonly struct ComponentItem
{
    internal readonly   InspectorComponent  control;
    internal readonly   Panel               panel;
    internal readonly   ComponentField[]    fields;
    
    internal ComponentItem(InspectorComponent control, Panel panel, ComponentField[] fields) {
        this.control    = control;
        this.panel      = panel;
        this.fields     = fields;
    }
} 


internal class InspectorObserver : EditorObserver
{
    private readonly    InspectorControl                            inspector;
    private readonly    Dictionary<TagType,       InspectorTag>     tagMap;
    private readonly    Dictionary<ComponentType, ComponentItem>    componentMap;
    private readonly    Dictionary<Type,          ComponentItem>    scriptMap;
    
    
    internal InspectorObserver (InspectorControl inspector, Editor editor) : base (editor)
    {
        this.inspector  = inspector;
        tagMap          = new Dictionary<TagType,       InspectorTag>();
        componentMap    = new Dictionary<ComponentType, ComponentItem>();
        scriptMap       = new Dictionary<Type,          ComponentItem>();
    }

    protected override void OnSelectionChanged(in EditorSelection selection)
    {
        var item    = selection.item;
        var entity  = item?.Entity;
        if (entity != null) {
            var archetype           = entity.Archetype;
            var model               = inspector.model;
            model.TagCount          = archetype.Tags.Count;
            model.ComponentCount    = archetype.Structs.Count;
            model.ScriptCount       = entity.Scripts.Length;
            AddEntityControls(entity);
        }
    }
    
    private void AddEntityControls(Entity entity)
    {
        // Console.WriteLine($"--- Inspector entity: {entity}");
        SetTags         (entity);
        SetComponents   (entity);
        SetScripts      (entity);
    }
    
    private void SetTags(Entity entity)
    {
        var tags = inspector.Tags.Children;
        tags.Clear();
        var archetype = entity.Archetype;
        foreach (var tagType in archetype.Tags)
        {
            if (!tagMap.TryGetValue(tagType, out var item)) {
                var tag = new Tags(tagType);
                item = new InspectorTag { TagName = tagType.tagName, EntityTag = tag };
                tagMap.Add(tagType, item);
            }
            item.Entity = entity;
            tags.Add(item);
        }
        inspector.TagGroup.GroupAdd.Entity = entity;
    }
    
    private void SetComponents(Entity entity)
    {
        var components  = inspector.Components.Children;
        components.Clear();  // todo optimize
        var archetype   = entity.Archetype;
        
        foreach (var componentType in archetype.Structs)
        {
            if (!componentMap.TryGetValue(componentType, out var item)) {
                var component   = new InspectorComponent { ComponentTitle = componentType.name, ComponentType = componentType };
                var panel       = new StackPanel();
                var fields      = AddComponentFields(componentType.type, panel);
                panel.Children.Add(new Separator());
                
                // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
                var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
                // ^-- same as: AvaloniaObjectExtensions.GetObservable(component, InspectorComponent.ExpandedProperty);
                panel.Bind(Visual.IsVisibleProperty, expanded);
                
                item = new ComponentItem(component, panel, fields);
                componentMap.Add(componentType, item);
            }          
            var instance = Entity.GetEntityComponent(entity, componentType); // todo - instance is a struct -> avoid boxing
            ComponentField.SetComponentFields(item.fields, entity, instance);
            item.control.Entity = entity;
            
            components.Add(item.control);
            components.Add(item.panel);
        }
        inspector.ComponentGroup.GroupAdd.Entity = entity;
    }
    
    private void SetScripts(Entity entity)
    {
        var scripts = inspector.Scripts.Children;
        scripts.Clear();  // todo optimize
        
        foreach (var script in entity.Scripts)
        {
            var type = script.GetType();
            if (!scriptMap.TryGetValue(type, out var item)) {
                var scriptType  = EntityStore.GetEntitySchema().ScriptTypeByType[type];
                var component   = new InspectorComponent { ComponentTitle = type.Name, ScriptType = scriptType };
                var panel       = new StackPanel();
                var fields      = new List<ComponentField>();
                ComponentField.AddScriptFields(fields, type);
                AddFields(fields, panel);
                panel.Children.Add(new Separator());
                // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
                var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
                panel.Bind(Visual.IsVisibleProperty, expanded);
                
                item = new ComponentItem(component, panel, fields.ToArray());
                scriptMap.Add(type, item);
            }
            ComponentField.SetScriptFields(item.fields, script);
            item.control.Entity = entity;
            
            scripts.Add(item.control);
            scripts.Add(item.panel);
        }
        inspector.ScriptGroup.GroupAdd.Entity = entity;
    }
    
    /// <remarks><see cref="SchemaType.type"/> is a struct</remarks>
    private static ComponentField[] AddComponentFields(Type type, Panel panel)
    {
        var fields = new List<ComponentField>();
        if (!ComponentField.AddComponentFields(fields, type, null, default)) {
            ComponentField.AddScriptFields(fields, type);
        }
        AddFields(fields, panel);
        return fields.ToArray();
    }
    
    private static void AddFields(List<ComponentField> fields, Panel panel)
    {
        foreach (var field in fields)
        {
            var dock    = new DockPanel();
            dock.Children.Add(new FieldName   { Text  = field.name } );
            dock.Children.Add(field.control);
            panel.Children.Add(dock);
        }
    }
}
