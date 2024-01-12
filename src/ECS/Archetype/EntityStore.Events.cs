// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
#region entity events - experimental
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
        var entityHandler = store.internBase.entityTagsChanged;
        if (entityHandler == null) {
            entityHandler = store.internBase.entityTagsChanged = new Dictionary<int, Action<TagsChangedArgs>[]>();
        }
        if (entityHandler.Count == 0) {
            store.internBase.tagsChanged += store.EntityTagsChanged;
        }
        if (entityHandler.TryGetValue(entityId, out var handlers)) {
            var newHandlers = new Action<TagsChangedArgs>[handlers.Length + 1];
            handlers.CopyTo(newHandlers, 0);
            return;
        }
        entityHandler.Add(entityId, [handler]);
    }
    
    internal static void RemoveEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChangedArgs> handler)
    {
        var entityHandler = store.internBase.entityTagsChanged;
        if (entityHandler == null) {
            return;
        }
        if (!entityHandler.TryGetValue(entityId, out var handlers)) {
            return;
        }
        int index = Array.FindIndex(handlers, item => item == handler);
        if (index == -1) {
            return;
        }
        var newLength = handlers.Length - 1;
        if (newLength > 0) {
            var newHandler  = new Action<TagsChangedArgs>[newLength];
            // --- remove handler at index
            for (int n = 0; n < index; n++) {
                newHandler[n] = handlers[n];
            }
            for (int n = index + 1; n <= newLength; n++) {
                newHandler[n - 1] = handlers[n];
            }
            entityHandler[entityId] = newHandler;
            return;
        }
        entityHandler.Remove(entityId);
        if (entityHandler.Count == 0) {
            store.internBase.tagsChanged -= store.EntityTagsChanged;
        }
    }
    #endregion
}
