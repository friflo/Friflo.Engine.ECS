// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal struct StoreIndex
{
    /// <summary> component index created on demand. </summary>
    private             ComponentIndex          index;  //  8
    internal readonly   IndexedComponentType    type;   // 24

    public override string ToString() {
        if (type.componentType == null) {
            return null;
        }
        var name = type.componentType.Name;
        if (index == null) {
            return name;
        }
        return $"{name} - {index.GetType().Name} count: {index.Count}";
    }

    internal StoreIndex(IndexedComponentType type) {
        this.type   = type;
    }
    
    internal static ComponentIndex GetIndex(EntityStore store, int structIndex)
    {
        var index = store.extension.indexes[structIndex].index;
        if (index != null) {
            return index;
        }
        ref var storeIndex      = ref store.extension.indexes[structIndex];
        return storeIndex.index = storeIndex.type.CreateComponentIndex(store);
    }
}
