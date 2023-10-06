// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct ForEachQuery<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private readonly ArchetypeQuery<T1,T2>  query;
    private readonly Action<T1, T2>         lambda;
    
    internal ForEachQuery(ArchetypeQuery<T1,T2> query, Action<T1, T2> lambda) {
        this.query  = query;
        this.lambda = lambda;
    }

    public void Run()
    {
        foreach (var archetype in query.Archetypes)
        {
            var heapMap     = archetype.heapMap;
            var entityCount = archetype.EntityCount;
            var heap1       = (StructHeap<T1>)heapMap[query.structIndexes.T1];
            var heap2       = (StructHeap<T2>)heapMap[query.structIndexes.T2];
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
