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
        foreach (var handler in handlers) {
            handler(args);
        }
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
    
#region generic add / remove event handler - experimental
    private static bool AddEntityHandler<TArgs>(
            int                                 entityId,
            Action<TArgs>                       handler,
        ref Dictionary<int, Action<TArgs>[]>    entityHandlerMap
        )
    {
        bool addEventHandler = false;
        var entityHandler = entityHandlerMap;
        if (entityHandler == null) {
            entityHandler = entityHandlerMap = new Dictionary<int, Action<TArgs>[]>();
        }
        if (entityHandler.Count == 0) {
            addEventHandler = true;
        }
        if (entityHandler.TryGetValue(entityId, out var handlers)) {
            // --- add handler to newHandlers[]
            var newHandlers = new Action<TArgs>[handlers.Length + 1];
            newHandlers[handlers.Length] = handler;
            handlers.CopyTo(newHandlers, 0);
            return addEventHandler;
        }
        entityHandler.Add(entityId, [handler]);
        return addEventHandler;
    }
    
    private static bool RemoveEntityHandler<TArgs>(
        int                                 entityId,
        Action<TArgs>                       handler,
        Dictionary<int, Action<TArgs>[]>    entityHandler)
    {
        if (entityHandler == null) {
            return false;
        }
        if (!entityHandler.TryGetValue(entityId, out var handlers)) {
            return false;
        }
        int index = Array.FindIndex(handlers, item => item == handler);
        if (index == -1) {
            return false;
        }
        var newLength = handlers.Length - 1;
        if (newLength > 0) {
            var newHandler  = new Action<TArgs>[newLength];
            // --- remove handler at index
            for (int n = 0; n < index; n++) {
                newHandler[n] = handlers[n];
            }
            for (int n = index + 1; n <= newLength; n++) {
                newHandler[n - 1] = handlers[n];
            }
            entityHandler[entityId] = newHandler;
            return false;
        }
        entityHandler.Remove(entityId);
        return entityHandler.Count == 0;
    }
    #endregion
}
