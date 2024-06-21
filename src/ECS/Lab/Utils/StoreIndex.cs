// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class StoreIndex
{
    internal static ComponentIndex GetIndex(EntityStore store, int structIndex)
    {
        var indexMap = store.extension.indexMap; 
        if (indexMap != null) {
            var index = indexMap[structIndex];
            if (index != null) {
                return index;
            }
            return indexMap[structIndex] = CreateIndex(store, structIndex);
        }
        indexMap = store.extension.indexMap = CreateStoreIndexMap();
        return indexMap[structIndex]  = CreateIndex(store, structIndex);
    }
    
    private static ComponentIndex CreateIndex(EntityStore store, int structIndex)
    {
        var type = EntityStoreBase.Static.EntitySchema.indexedComponentMap[structIndex];
        return type.CreateComponentIndex(store);
    }
    
    private static ComponentIndex[] CreateStoreIndexMap()
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return new ComponentIndex[schema.maxStructIndex]; // could create smaller array containing no null elements
    }
}
