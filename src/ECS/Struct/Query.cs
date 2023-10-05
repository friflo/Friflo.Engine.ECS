// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class ArchetypeQuery
{
#region private fields
    private  readonly   EntityStore     store;
    private  readonly   ArchetypeMask   mask;
    private             int             lastArchetypeCount;
    private             Archetype[]     archetypes;
    internal readonly   int[]           structIndices;  // todo use index1, index2, index3, ...   
    private             int             archetypeCount;
    #endregion
    
    internal ArchetypeQuery(EntityStore store, Signature signature) {
        this.store          = store;
        archetypes          = new Archetype[1];
        mask                = new ArchetypeMask(signature);
        lastArchetypeCount  = 1;
        var componentTypes  = signature.componentTypes;
        structIndices       = new int[componentTypes.Length];
        for (int n = 0; n < componentTypes.Length; n++) {
            structIndices[n] = componentTypes[n].index;
        }
    }
    
    public ReadOnlySpan<Archetype> Archetypes {
        get
        {
            if (store.archetypesCount == lastArchetypeCount) {
                return new ReadOnlySpan<Archetype>(archetypes, 0, archetypeCount);
            }
            var storeArchetypes = store.Archetypes;
            var newCount        = storeArchetypes.Length;
            for (int n = lastArchetypeCount; n < newCount; n++) {
                var archetype = storeArchetypes[n];
                if (!mask.Has(archetype.mask)) {
                    continue;
                }
                if (archetypeCount == archetypes.Length) {
                    Utils.Resize(ref archetypes, 2 * archetypeCount);
                }
                archetypes[archetypeCount++] = archetype;
            }
            lastArchetypeCount = newCount;
            return new ReadOnlySpan<Archetype>(archetypes, 0, archetypeCount);
        }
    }
}


public sealed class ArchetypeQuery<T> : ArchetypeQuery
    where T : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T> signature)
        : base(store, signature) {
    }
}

public sealed class ArchetypeQuery<T1, T2> : ArchetypeQuery // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct
    where T2 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2> signature)
        : base(store, signature) {
    }
    
    public QueryEnumerator2<T1,T2> GetEnumerator() => new QueryEnumerator2<T1,T2>(this);
    
    public void ForEach(Action<T1, T2> lambda)
    {
        foreach (var archetype in Archetypes)
        {
            var indices     = structIndices;
            var heapMap     = archetype.heapMap;
            var entityCount = archetype.EntityCount;
            var heap1       = (StructHeap<T1>)heapMap[indices[0]];
            var heap2       = (StructHeap<T2>)heapMap[indices[1]];
            for (int n = 0; n < entityCount; n++)
            {
                // todo unroll loop
                var component1 = heap1.chunks[n / ChunkSize].components[n % ChunkSize];
                var component2 = heap2.chunks[n / ChunkSize].components[n % ChunkSize];
                lambda(component1, component2);
            }
        }
    }
}

public sealed class ArchetypeQuery<T1, T2, T3> : ArchetypeQuery
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2, T3> signature)
        : base(store, signature) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4> : ArchetypeQuery
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2, T3, T4> signature)
        : base(store, signature) {
    }
}

public sealed class ArchetypeQuery<T1, T2, T3, T4, T5> : ArchetypeQuery
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
{
    internal ArchetypeQuery(EntityStore store, Signature<T1, T2, T3, T4, T5> signature)
        : base(store, signature) {
    }
}
