// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class StoreIndex
{
    internal static void UpdateIndex<TComponent>(EntityStoreBase store, int id, in TComponent component, StructHeap<TComponent> heap)
        where TComponent : struct, IComponent
    {
        var index = GetIndex((EntityStore)store, StructInfo<TComponent>.Index);
        index.Update(id, component, heap);
    }
    
    internal static void AddIndex<TComponent>(EntityStoreBase store, int id, in TComponent component)
        where TComponent : struct, IComponent
    {
        var index = GetIndex((EntityStore)store, StructInfo<TComponent>.Index);
        index.Add(id, component);
    }
    
    internal static void RemoveIndex<TComponent>(EntityStoreBase store, int id, StructHeap<TComponent> heap)
        where TComponent : struct, IComponent
    {
        var index = GetIndex((EntityStore)store, StructInfo<TComponent>.Index);
        index.Remove(id, heap);
    }
    
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
        var componentType = EntityStoreBase.Static.EntitySchema.components[structIndex];
        return ComponentIndexUtils.CreateComponentIndex(store, componentType);
    }
    
    private static ComponentIndex[] CreateStoreIndexMap()
    {
        var schema = EntityStoreBase.Static.EntitySchema;
        return new ComponentIndex[schema.maxIndexedStructIndex];
    }
}
