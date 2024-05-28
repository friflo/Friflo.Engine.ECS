// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Friflo.Editor.Utils;
using Friflo.Engine.ECS;

// ReSharper disable ParameterTypeCanBeEnumerable.Local
namespace Friflo.Editor.UI.Inspector;


internal readonly struct ComponentItem
{
    internal readonly   InspectorComponent  inspectorComponent;
    internal readonly   Panel               componentPanel;
    internal readonly   ComponentField[]    fields;
    
    internal ComponentItem(InspectorComponent inspectorComponent, Panel componentPanel, ComponentField[] fields) {
        this.inspectorComponent = inspectorComponent;
        this.componentPanel     = componentPanel;
        this.fields             = fields;
    }
} 


internal class InspectorObserver : EditorObserver
{
    private readonly    InspectorControl                            inspector;
    private readonly    Dictionary<TagType,       InspectorTag>     tagMap;
    private readonly    Dictionary<ComponentType, ComponentItem>    componentMap;
    private readonly    Dictionary<ScriptType,    ComponentItem>    scriptMap;
    private readonly    HashSet<Control>                            controlSet;
    private readonly    List<Control>                               controlList;
    private readonly    EntitySchema                                schema;
    private             int                                         entityId;
    private             SchemaType                                  focusSchemaType;
    
    
    internal InspectorObserver (InspectorControl inspector, AppEvents appEvents) : base (appEvents)
    {
        this.inspector  = inspector;
        tagMap          = new Dictionary<TagType,       InspectorTag>();
        componentMap    = new Dictionary<ComponentType, ComponentItem>();
        scriptMap       = new Dictionary<ScriptType,    ComponentItem>();
        controlSet      = new HashSet<Control>();
        controlList     = new List<Control>();
        schema          = EntityStore.GetEntitySchema();
    }
    
    protected override void OnEditorReady() {
        var store = Store;
        store.OnComponentAdded      += args => PostSetEntity(args.EntityId); 
        store.OnComponentRemoved    += args => PostSetEntity(args.EntityId); 
        store.OnTagsChanged         += args => PostSetEntity(args.EntityId);
        store.OnScriptAdded         += args => PostSetEntity(args.Entity.Id);
        store.OnScriptRemoved       += args => PostSetEntity(args.Entity.Id); 
        store.OnEntitiesChanged     += EntitiesChanged;
    }
    
    private void PostSetEntity(int id)
    {
        if (id != entityId) {
            return;
        }
        StoreDispatcher.Post(() => {
            SetEntity(id);
        });
    }
    
    private void EntitiesChanged(object sender, EntitiesChanged args) {
        if (!args.EntityIds.Contains(entityId)) {
            return;
        }
        // could Post() change event
        SetEntity(entityId);
    }

    protected override void OnSelectionChanged(in EditorSelection selection)
    {
        var item    = selection.item;
        if (item == null) {
            return;
        }
        var entity  = item.Entity;
        if (entity.IsNull) {
            return;
        }
        SetEntity(entity.Id);
    }
    
    private void SetEntity(int id)
    {
        // Console.WriteLine($"--- Inspector entity: {entity}");
        entityId                = id;
        var entity              = Store.GetEntityById(id);
        var archetype           = entity.Archetype;
        var model               = inspector.model;
        model.EntityId          = id;
        model.TagCount          = archetype.Tags.Count;
        model.ComponentCount    = archetype.ComponentTypes.Count;
        model.ScriptCount       = entity.Scripts.Length;
        
        SetTags         (entity);
        SetComponents   (entity);
        SetScripts      (entity);
        
        SetComponentFocus();
    }
    
    internal void FocusComponent(SchemaType schemaType) {
        focusSchemaType = schemaType;
    }
    
    private void SetComponentFocus()
    {
        var focus = focusSchemaType;
        if (focus == null) {
            return;
        }
        focusSchemaType = null;
        if (focus is ComponentType componentType) {
            var panel = componentMap[componentType].componentPanel;
            FocusPanel(panel);
            return;
        }
        if (focus is ScriptType scriptType) {
            var panel = scriptMap[scriptType].componentPanel;
            FocusPanel(panel);
        }
    }
    
    private static void FocusPanel(Panel panel) {
        StoreDispatcher.Post(() => {
            var focusable = EditorUtils.FindFocusable(panel);
            focusable?.Focus(NavigationMethod.Tab);
        });
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
                item = new InspectorTag { TagName = tagType.TagName, EntityTag = tag };
                tagMap.Add(tagType, item);
            }
            item.Entity = entity;
            tags.Add(item);
        }
        inspector.TagGroup.GroupAdd.Entity = entity;
    }
    
    private void SetComponents(Entity entity)
    {
        var controls    = InitControlSet(inspector.Components.Children);
        var archetype   = entity.Archetype;
        
        foreach (var componentType in archetype.ComponentTypes)
        {
            if (!componentMap.TryGetValue(componentType, out var item)) {
                var component   = new InspectorComponent { ComponentTitle = componentType.Name, ComponentType = componentType };
                var panel       = new StackPanel();
                var fields      = new List<ComponentField>();
                ComponentField.AddComponentTypeFields(fields, componentType);
                AddFields(fields, panel);
                panel.Children.Add(new Separator());
                
                // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
                var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
                // ^-- same as: AvaloniaObjectExtensions.GetObservable(component, InspectorComponent.ExpandedProperty);
                panel.Bind(Visual.IsVisibleProperty, expanded);
                
                item = new ComponentItem(component, panel, fields.ToArray());
                componentMap.Add(componentType, item);
            }          
            var instance = EntityUtils.GetEntityComponent(entity, componentType); // todo - instance is a struct -> avoid boxing
            ComponentField.SetComponentFields(item.fields, entity, instance);
            item.inspectorComponent.Entity = entity;
            
            controlList.Add(item.inspectorComponent);
            controlList.Add(item.componentPanel);
        }
        inspector.ComponentGroup.GroupAdd.Entity = entity;
        UpdateControls(controls);
    }
    
    private void SetScripts(Entity entity)
    {
        var controls = InitControlSet(inspector.Scripts.Children);
        
        foreach (var script in entity.Scripts)
        {
            var type        = script.GetType();
            var scriptType  = schema.ScriptTypeByType[type];
            if (!scriptMap.TryGetValue(scriptType, out var item))
            {
                var component   = new InspectorComponent { ComponentTitle = type.Name, ScriptType = scriptType };
                var panel       = new StackPanel();
                var fields      = new List<ComponentField>();
                ComponentField.AddScriptFields(fields, null, type);
                AddFields(fields, panel);
                panel.Children.Add(new Separator());
                // <StackPanel IsVisible="{Binding #Comp1.Expanded}"
                var expanded = component.GetObservable(InspectorComponent.ExpandedProperty);
                panel.Bind(Visual.IsVisibleProperty, expanded);
                
                item = new ComponentItem(component, panel, fields.ToArray());
                scriptMap.Add(scriptType, item);
            }
            ComponentField.SetScriptFields(entity, item.fields, script);
            item.inspectorComponent.Entity = entity;
            
            controlList.Add(item.inspectorComponent);
            controlList.Add(item.componentPanel);
        }
        inspector.ScriptGroup.GroupAdd.Entity = entity;
        UpdateControls(controls);
    }
    
    private static void AddFields(List<ComponentField> fields, Panel panel)
    {
        foreach (var field in fields)
        {
            var dock    = new DockPanel();
            if (field.IsLabeled) {
                dock.Children.Add(new FieldLabel   { Text  = field.name } );
            }
            dock.Children.Add(field.control);
            panel.Children.Add(dock);
        }
    }
    
    private Controls InitControlSet(Controls controls)
    {
        controlList.Clear();
        return controls;
    }
    
    /// <remarks>
    /// <see cref="controlList"/> items layout
    /// <code>
    ///     [0] InspectorComponent
    ///     [1] Panel
    ///     [2] InspectorComponent
    ///     [3] Panel
    ///     ...
    /// </code>
    /// </remarks>
    private void UpdateControls(Controls controls)
    {
        controlSet.Clear();
        foreach (var control in controls) {
            controlSet.Add(control);
        }
        InspectorComponent inspectorComponent = null; 
        foreach (var control in controlList)
        {
            bool isInspectorComponent = false;
            if (control is InspectorComponent component) {
                inspectorComponent      = component;
                isInspectorComponent    = true;
            }
            if (control.Parent == null) {
                controls.Add(control);
            } else {
                if (isInspectorComponent) {
                    inspectorComponent.IsVisible = true;
                } else {
                    // case: control is componentPanel
                    control.IsVisible = inspectorComponent!.Expanded;
                }
            }
            controlSet.Remove(control);
        }
        foreach (var control in controlSet) {
            control.IsVisible = false;
        }
    }
}
