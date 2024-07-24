// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        if (AddEntityHandler(store, entityId, handler, HasEventFlags.TagsChanged, ref store.internBase.entityTagsChanged)) {
            store.internBase.tagsChanged += store.EntityTagsChanged;
        }
    }
    
    internal static void RemoveEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChanged> handler)
    {
        if (RemoveEntityHandler(store, entityId, handler, HasEventFlags.TagsChanged, store.internBase.entityTagsChanged)) {
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
    
    internal static void AddComponentChangedHandler(EntityStore store, int entityId, Action<ComponentChanged> handler)
    {
        if (AddEntityHandler(store, entityId, handler, HasEventFlags.ComponentChanged,ref store.internBase.entityComponentChanged)) {
            store.internBase.componentAdded     += store.ComponentChanged;
            store.internBase.componentRemoved   += store.ComponentChanged;
        }
    }
    
    internal static void RemoveComponentChangedHandler(EntityStore store, int entityId, Action<ComponentChanged> handler)
    {
        if (RemoveEntityHandler(store, entityId, handler, HasEventFlags.ComponentChanged, store.internBase.entityComponentChanged)) {
            store.internBase.componentAdded     -= store.ComponentChanged;
            store.internBase.componentRemoved   -= store.ComponentChanged;
        }
    }
    #endregion
    
    
    
#region generic add / remove event handler
    internal static bool AddEntityHandler<TArgs>(
            EntityStoreBase                 store,
            int                             entityId,
            Action<TArgs>                   handler,
            HasEventFlags                   hasEvent,
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
        ((EntityStore)store).nodes[entityId].hasEvent |= hasEvent;
        entityHandler.Add(entityId, handler);
        return addEventHandler;
    }
    
    internal static bool RemoveEntityHandler<TArgs>(
        EntityStoreBase                 store,
        int                             entityId,
        Action<TArgs>                   handler,
        HasEventFlags                   hasEvent,
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
        ((EntityStore)store).nodes[entityId].hasEvent &= ~hasEvent;
        entityHandler.Remove(entityId);
        return entityHandler.Count == 0;
    }
    #endregion
    
    internal static void RemoveAllEntityEventHandlers(EntityStore store, int entityId, HasEventFlags hasEvent)
    {
        if ((hasEvent & HasEventFlags.ComponentChanged) != 0) {
            var handlerMap = store.internBase.entityComponentChanged;
            handlerMap.Remove(entityId);
            if (handlerMap.Count == 0) {
                store.internBase.componentAdded     -= store.ComponentChanged;
                store.internBase.componentRemoved   -= store.ComponentChanged;    
            }
        }
        if ((hasEvent & HasEventFlags.TagsChanged) != 0) {
            var handlerMap = store.internBase.entityTagsChanged;
            handlerMap.Remove(entityId);
            if (handlerMap.Count == 0) {
                store.internBase.tagsChanged        -= store.EntityTagsChanged;
            }
        }
    }
    
    [ExcludeFromCodeCoverage]
    internal static void AssertEventDelegatesNull(EntityStore store)
    {
        if (store.internBase.componentAdded     != null) throw new InvalidOperationException("expect null");
        if (store.internBase.componentRemoved   != null) throw new InvalidOperationException("expect null");
        if (store.internBase.tagsChanged        != null) throw new InvalidOperationException("expect null");
    }
    
    internal static void AddEventHandlers(ref List<DebugEventHandler> eventHandlers, EntityStore store, int entityId, HasEventFlags hasEvent)
    {
        if ((hasEvent & HasEventFlags.ComponentChanged) != 0) {
            var handlers    = store.internBase.entityComponentChanged[entityId];
            var handler     = new DebugEventHandler(DebugEntityEventKind.Event, typeof(ComponentChanged), handlers.GetInvocationList());
            eventHandlers ??= new List<DebugEventHandler>();
            eventHandlers.Add(handler);
        }
        if ((hasEvent & HasEventFlags.TagsChanged) != 0) {
            var handlers    = store.internBase.entityTagsChanged[entityId];
            var handler     = new DebugEventHandler(DebugEntityEventKind.Event, typeof(TagsChanged), handlers.GetInvocationList());
            eventHandlers ??= new List<DebugEventHandler>();
            eventHandlers.Add(handler);
        }
    }
}
