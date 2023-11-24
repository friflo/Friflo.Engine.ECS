// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;

// ReSharper disable ParameterTypeCanBeEnumerable.Local
namespace Friflo.Fliox.Editor.UI.Inspector;

internal class InspectorObserver : EditorObserver
{
    private readonly InspectorControl inspector;
    
    
    internal InspectorObserver (InspectorControl inspector, Editor editor) : base (editor)
    {
        this.inspector = inspector;
    }

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
    
    /// <remarks><see cref="ComponentType.type"/> is a struct</remarks>
    private static void AddComponentFields(Entity entity, ComponentType componentType, Panel panel)
    {
        var instance        = entity.Archetype.GetEntityComponent(entity, componentType); // todo - instance is a struct -> avoid boxing
        var fields          = new List<ComponentField>();
        ComponentField.AddComponentFields(fields, componentType.type, instance, null);
        AddFields(fields, panel);
        panel.Children.Add(new Separator());
    }
    
    /// <remarks><paramref name="script"/> is a class</remarks>
    private static void AddScriptFields(Script script, Panel panel)
    {
        var scriptType  = script.GetType();
        var fields      = new List<ComponentField>();
        ComponentField.AddComponentFields(fields, scriptType, script, null);
        AddFields(fields, panel);
        panel.Children.Add(new Separator());
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
