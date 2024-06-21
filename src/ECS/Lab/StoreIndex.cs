// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal struct StoreIndex
{
    public   override   string          ToString()  => GetString();
    
    /// <summary> component index created on demand. </summary>
    private             ComponentIndex  index;          //  8
    
    /// <summary> only stored for debugging </summary>
    private  readonly   int             structIndex;    //  4

    private StoreIndex(int structIndex) {
        this.structIndex = structIndex;
    }
    
    internal static ComponentIndex GetIndex(EntityStore store, int structIndex)
    {
        var indexMap = store.extension.indexMap; 
        if (indexMap != null) {
            var index = indexMap[structIndex].index;
            if (index != null) {
                return index;
            }
            return indexMap[structIndex].index = CreateIndex(store, structIndex);
        }
        indexMap = store.extension.indexMap = CreateStoreIndexMap();
        return indexMap[structIndex].index  = CreateIndex(store, structIndex);
    }
    
    private static ComponentIndex CreateIndex(EntityStore store, int structIndex)
    {
        var type = EntityStoreBase.Static.EntitySchema.indexedComponentMap[structIndex];
        return type.CreateComponentIndex(store);
    }
    
    private static StoreIndex[] CreateStoreIndexMap()
    {
        var schema          = EntityStoreBase.Static.EntitySchema;
        var storeIndexes    = new StoreIndex[schema.maxStructIndex]; // could create smaller array containing no null elements
        foreach (var type in schema.indexedComponents) {
            storeIndexes[type.componentType.StructIndex] = new StoreIndex(type.componentType.StructIndex);
        }
        return storeIndexes;
    }
    
    private string GetString()
    {
        var type = EntityStoreBase.Static.EntitySchema.indexedComponentMap[structIndex];
        if (type.componentType == null) {
            return null;
        }
        var name = type.componentType.Name;
        if (index == null) {
            return name;
        }
        return $"{name} - {index.GetType().Name} count: {index.Count}";
    }
}
