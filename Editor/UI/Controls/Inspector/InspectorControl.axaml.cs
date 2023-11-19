// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

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
            var name    = item?.Name ?? "no selection";
            var id      = item != null ? $"id: {item.Id}" : null;
            inspector.EntityName.Content    = name;
            inspector.EntityId.Content      = id;
            
            var entity = item?.Entity;
            if (entity != null) {
                AddComponentControls(entity);
            }
        }
        
        private void AddComponentControls(GameEntity entity)
        {
            // Console.WriteLine($"--- Inspector entity: {entity}");
            var children = inspector.Components.Children;
            children.Clear();
            var archetype = entity.Archetype;
            foreach (var componentType in archetype.Structs)
            {
                var value   = archetype.GetEntityComponent(entity, componentType);
                var text    = value.ToString();
                var label   = new Label { Content = text };
                DockPanel.SetDock(label, Dock.Top);
                children.Add(label);
                // Console.WriteLine(text);
            }
        }
    }
}