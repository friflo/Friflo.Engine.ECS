// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly ref struct EntityState
{
#region entity getter
    public  bool        IsNull          => archetype == null;
    
    public  Tags        Tags            => archetype.tags;
    
    public  ref T       Get<T>() where T : struct, IComponent {
        return ref ((StructHeap<T>)archetype.heapMap[StructInfo<T>.Index]).components[compIndex];
    }
    #endregion
    
#region fields
    private readonly    Archetype   archetype;
    private readonly    int         compIndex;
#endregion

    internal EntityState(Archetype archetype, int compIndex) {
        this.archetype  = archetype;
        this.compIndex  = compIndex;
    }
}