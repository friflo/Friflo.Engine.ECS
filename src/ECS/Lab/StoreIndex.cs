// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal struct StoreIndex
{
    public   override   string          ToString()  => GetString();
    
    /// <summary> component index created on demand. </summary>
    private             ComponentIndex  index;          //  8
    private  readonly   int             structIndex;    //  4

    internal StoreIndex(int structIndex) {
        this.structIndex = structIndex;
    }
    
    internal static ComponentIndex GetIndex(EntityStore store, int structIndex)
    {
        var index = store.extension.indexes[structIndex].index;
        if (index != null) {
            return index;
        }
        var type = EntityStoreBase.Static.EntitySchema.indexedComponentMap[structIndex];
        return store.extension.indexes[structIndex].index = type.CreateComponentIndex(store);
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
