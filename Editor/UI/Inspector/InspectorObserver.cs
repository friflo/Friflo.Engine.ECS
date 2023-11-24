// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable ReturnTypeCanBeEnumerable.Local
namespace Friflo.Fliox.Editor.UI.Inspector;

internal class InspectorObserver : EditorObserver
{
    private readonly InspectorControl inspector;
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore

    
    internal InspectorObserver (InspectorControl inspector, Editor editor) : base (editor) { this.inspector = inspector; }

    protected override void OnSelectionChanged(in EditorSelection selection)
    {
        var item    = selection.item;
        var entity  = item?.Entity;
        if (entity != null) {
            AddEntityControls(entity);
        }
    }
    
    private void AddEntityControls(Entity entity)
    {
        // Console.WriteLine($"--- Inspector entity: {entity}");
        var tags        = inspector.Tags.Children;
        var components  = inspector.Components.Children;
        var scripts     = inspector.Scripts.Children;
        tags.Clear();
        components.Clear();
        scripts.Clear();
        var archetype = entity.Archetype;
        
        // --- tags
        foreach (var tagName in archetype.Tags) {
            tags.Add(new InspectorTag { TagName = tagName.tagName });
        }
        // --- components
        foreach (var componentType in archetype.Structs)
        {
            var component = new InspectorComponent { ComponentTitle = componentType.type.Name };
            components.Add(component);
            var panel = new StackPanel();
            AddComponentFields(entity, componentType, panel);
            
            // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
            var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
            // ^-- same as: AvaloniaObjectExtensions.GetObservable(component, InspectorComponent.ExpandedProperty);
            panel.Bind(Visual.IsVisibleProperty, expanded);
            
            components.Add(panel);
        }
        // --- scripts
        foreach (var script in entity.Scripts)
        {
            var component = new InspectorComponent { ComponentTitle = script.GetType().Name };
            scripts.Add(component);
            var panel = new StackPanel();
            AddScriptFields(script, panel);
            
            // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
            var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
            panel.Bind(Visual.IsVisibleProperty, expanded);
            
            scripts.Add(panel);
        }
    }
    
    private static void AddComponentFields(Entity entity, ComponentType componentType, Panel panel)
    {
        var instance    = entity.Archetype.GetEntityComponent(entity, componentType);
        var fields      = GetComponentFields(componentType.type);
        
        foreach (var field in fields) {
            var dock    = new DockPanel();
            var value   = field.member.GetVar(instance);
            dock.Children.Add(new FieldName   { Text  = field.field.name } );
            dock.Children.Add(new StringField { Value = value.AsString() } );
            panel.Children.Add(dock);
        }
    }
    
    private static void AddScriptFields(Script script, Panel panel)
    {
        var scriptType  = script.GetType();
        var fields      = GetComponentFields(scriptType);
        
        foreach (var field in fields) {
            var dock    = new DockPanel();
            var value   = field.member.GetVar(script);
            dock.Children.Add(new FieldName   { Text  = field.field.name } );
            dock.Children.Add(new StringField { Value = value.AsString() } );
            panel.Children.Add(dock);
        }
    }
    
    private static ComponentField[] GetComponentFields(Type type)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var fields      = classMapper.PropFields.fields;
        var result      = new ComponentField[fields.Length];
        
        for (int n = 0; n < fields.Length; n++) {
            var propField   = fields[n];
            var member      = classMapper.GetMember(propField.name);
            result[n]       = new ComponentField { field = propField, member = member };
        }
        return result;
    }
}

internal struct ComponentField
{
    internal    PropField   field;
    /// <summary>Access member value with <see cref="Var.Member.GetVar"/></summary>
    internal    Var.Member  member;
}
