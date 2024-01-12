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
        if (!internBase.entityTagsChanged.TryGetValue(args.entityId, out var handler)) {
            return;
        }
        handler(args);
    }
    
    internal static void AddEntityTagsChangedHandler(EntityStoreBase store, int entityId, Action<TagsChangedArgs> handler)
    {
        var entityHandler = store.internBase.entityTagsChanged;
        if (entityHandler == null) {
            entityHandler = store.internBase.entityTagsChanged = new Dictionary<int, Action<TagsChangedArgs>>();
        }
        if (entityHandler.Count == 0) {
            store.OnTagsChanged += store.EntityTagsChanged;
        }
        entityHandler.Add(entityId, handler);
    }
    #endregion
}
