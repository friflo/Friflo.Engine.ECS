// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class InspectorControl : UserControl
{
    public InspectorControl()
    {
        InitializeComponent();
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        var editor = this.GetEditor();
        editor?.AddObserver(new InspectorObserver(this, editor));
    }
    
    // ------------------------------------------- EditorObserver -------------------------------------------
    private class InspectorObserver : EditorObserver
    {
        private readonly InspectorControl inspector;
        
        internal InspectorObserver (InspectorControl inspector, Editor editor) : base (editor) { this.inspector = inspector; }

        protected override void OnSelectionChanged(in EditorSelection selection)
        {
            var item    = selection.item;
            // var name    = item?.Name ?? "no selection";
            // var id      = item != null ? $"id: {item.Id}" : null;
            
            // inspector.EntityName.Content    = name;
            // inspector.EntityId.Content      = id;
            
            var entity = item?.Entity;
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
                // same as: AvaloniaObjectExtensions.GetObservable(component, InspectorComponent.ExpandedProperty);
                panel.Bind(IsVisibleProperty, expanded);
                
                components.Add(panel);
            }
            // --- scripts
            foreach (var script in entity.Scripts)
            {
                var component = new InspectorComponent { ComponentTitle = script.GetType().Name };
                scripts.Add(component);
                var panel = new StackPanel();
                AddScriptFields(entity, script, panel);
                
                // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
                var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
                panel.Bind(IsVisibleProperty, expanded);
                
                scripts.Add(panel);
            }
        }
        
        private static void AddComponentFields(Entity entity, ComponentType componentType, StackPanel panel)
        {
            for (int n = 1; n <= 2; n++) {
                var dock = new DockPanel();
                dock.Children.Add(new FieldName   { Text  = $"Field {n}"} );
                dock.Children.Add(new StringField { Value = $"value {n}"} );
                panel.Children.Add(dock);
            }
        }
        
        private static void AddScriptFields(Entity entity, Script script, StackPanel panel)
        {
            for (int n = 1; n <= 1; n++) {
                var dock = new DockPanel();
                dock.Children.Add(new FieldName   { Text  = $"Field {n}"} );
                dock.Children.Add(new StringField { Value = $"value {n}"} );
                panel.Children.Add(dock);
            }
        }
    }
}