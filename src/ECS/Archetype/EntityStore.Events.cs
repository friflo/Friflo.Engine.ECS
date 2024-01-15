// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
#region entity tag events
    private void EntityTagsChanged(TagsChanged args)
    {
        if (!internBase.entityTagsChanged.TryGetValue(args.EntityId, out var handlers)) {
            return;
        }
        handlers.Invoke(args);
    }
    
    internal static void AddEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChanged> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.internBase.entityTagsChanged)) {
            store.internBase.tagsChanged += store.EntityTagsChanged;
        }
    }
    
    internal static void RemoveEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChanged> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.internBase.entityTagsChanged)) {
            store.internBase.tagsChanged -= store.EntityTagsChanged;
        }
    }
    #endregion
    
    
    
#region add / remove component events
    private void ComponentChanged(ComponentChanged args)
    {
        if (!internBase.entityComponentChanged.TryGetValue(args.EntityId, out var handlers)) {
            return;
        }
        handlers.Invoke(args);
    }
    
    internal static void AddComponentChangedHandler(EntityStoreBase store, int entityId, Action<ComponentChanged> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.internBase.entityComponentChanged)) {
            store.internBase.componentAdded     += store.ComponentChanged;
            store.internBase.componentRemoved   += store.ComponentChanged;
        }
    }
    
    internal static void RemoveComponentChangedHandler(EntityStoreBase store, int entityId, Action<ComponentChanged> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.internBase.entityComponentChanged)) {
            store.internBase.componentAdded     -= store.ComponentChanged;
            store.internBase.componentRemoved   -= store.ComponentChanged;
        }
    }
    #endregion
    
    
    
#region generic add / remove event handler
    internal static bool AddEntityHandler<TArgs>(
            int                             entityId,
            Action<TArgs>                   handler,
        ref Dictionary<int, Action<TArgs>>  entityHandlerMap) where TArgs : struct
    {
        bool addEventHandler = false;
        var entityHandler = entityHandlerMap;
        if (entityHandler == null) {
            entityHandler = entityHandlerMap = new Dictionary<int, Action<TArgs>>();
        }
        if (entityHandler.Count == 0) {
            addEventHandler = true;
        }
        if (entityHandler.TryGetValue(entityId, out var handlers)) {
            handlers += handler;
            entityHandler[entityId] = handlers;
            return addEventHandler;
        }
        entityHandler.Add(entityId, handler);
        return addEventHandler;
    }
    
    internal static bool RemoveEntityHandler<TArgs>(
        int                             entityId,
        Action<TArgs>                   handler,
        Dictionary<int, Action<TArgs>>  entityHandler) where TArgs : struct
    {
        if (entityHandler == null) {
            return false;
        }
        if (!entityHandler.TryGetValue(entityId, out var handlers)) {
            return false;
        }
        handlers -= handler;
        if (handlers != null) {
            entityHandler[entityId] = handlers;
            return false;
        }
        entityHandler.Remove(entityId);
        return entityHandler.Count == 0;
    }
    #endregion
    
    internal static void AddEventHandlers(List<EventHandler> eventHandlers, EntityStore store, int entityId)
    {
        var entityComponentChanged = store.internBase.entityComponentChanged;
        if (entityComponentChanged != null) {
            if (entityComponentChanged.TryGetValue(entityId, out var handlers)) {
                var handler = new EventHandler(nameof(Entity.OnComponentChanged), typeof(ComponentChanged), handlers.GetInvocationList());
                eventHandlers.Add(handler);
            }
        }
        var entityTagsChanged = store.internBase.entityTagsChanged;
        if (entityTagsChanged != null) {
            if (entityTagsChanged.TryGetValue(entityId, out var handlers)) {
                var handler = new EventHandler(nameof(Entity.OnTagsChanged), typeof(TagsChanged), handlers.GetInvocationList());
                eventHandlers.Add(handler);
            }
        }
    }
}
