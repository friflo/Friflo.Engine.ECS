// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
#region entity tag events - experimental
    private void EntityTagsChanged(object sender, TagsChangedArgs args)
    {
        if (!internBase.entityTagsChanged.TryGetValue(args.entityId, out var handlers)) {
            return;
        }
        handlers.action?.Invoke(args);
    }
    
    internal static void AddEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChangedArgs> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.internBase.entityTagsChanged)) {
            store.internBase.tagsChanged += store.EntityTagsChanged;
        }
    }
    
    internal static void RemoveEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChangedArgs> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.internBase.entityTagsChanged)) {
            store.internBase.tagsChanged -= store.EntityTagsChanged;
        }
    }
    #endregion
    
    
    
#region add / remove component events - experimental
    private void ComponentChanged(object sender, ComponentChangedArgs args)
    {
        if (!internBase.entityComponentChanged.TryGetValue(args.entityId, out var handlers)) {
            return;
        }
        handlers.action?.Invoke(args);
    }
    
    internal static void AddComponentChangedHandler(EntityStoreBase store, int entityId, Action<ComponentChangedArgs> handler)
    {
        if (AddEntityHandler(entityId, handler, ref store.internBase.entityComponentChanged)) {
            store.internBase.componentAdded     += store.ComponentChanged;
            store.internBase.componentRemoved   += store.ComponentChanged;
        }
    }
    
    internal static void RemoveComponentChangedHandler(EntityStoreBase store, int entityId, Action<ComponentChangedArgs> handler)
    {
        if (RemoveEntityHandler(entityId, handler, store.internBase.entityComponentChanged)) {
            store.internBase.componentAdded     -= store.ComponentChanged;
            store.internBase.componentRemoved   -= store.ComponentChanged;
        }
    }
    #endregion
    
    
    
#region generic add / remove event handler - experimental
    internal static bool AddEntityHandler<TArgs>(
            int                             entityId,
            Action<TArgs>                   handler,
        ref Dictionary<int, Actions<TArgs>> entityHandlerMap) where TArgs : struct
    {
        bool addEventHandler = false;
        var entityHandler = entityHandlerMap;
        if (entityHandler == null) {
            entityHandler = entityHandlerMap = new Dictionary<int, Actions<TArgs>>();
        }
        if (entityHandler.Count == 0) {
            addEventHandler = true;
        }
        if (entityHandler.TryGetValue(entityId, out var handlers)) {
            handlers.action += handler;
            entityHandler[entityId] = handlers;
            return addEventHandler;
        }
        var actions = new Actions<TArgs> { action = handler };
        entityHandler.Add(entityId, actions);
        return addEventHandler;
    }
    
    internal static bool RemoveEntityHandler<TArgs>(
        int                                 entityId,
        Action<TArgs>                       handler,
        Dictionary<int, Actions<TArgs>>     entityHandler) where TArgs : struct
    {
        if (entityHandler == null) {
            return false;
        }
        if (!entityHandler.TryGetValue(entityId, out var handlers)) {
            return false;
        }
        handlers.action -= handler;
        if (handlers.action != null) {
            entityHandler[entityId] = handlers;
            return false;
        }
        entityHandler.Remove(entityId);
        return entityHandler.Count == 0;
    }
    #endregion
}


internal struct Actions<TArg> where TArg : struct
{
    internal Action<TArg> action;
}
