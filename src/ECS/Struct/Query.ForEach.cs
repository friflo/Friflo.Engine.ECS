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
    private readonly ArchetypeQuery<T1,T2>      query;
    private readonly Action<Ref<T1>, Ref<T2>>   lambda;
    
    internal ForEachQuery(
        ArchetypeQuery<T1, T2>      query,
        Action<Ref<T1>, Ref<T2>>    lambda)
    {
        this.query  = query;
        this.lambda = lambda;
    }

    public void Run()
    {
        var ref1 = new Ref<T1>();
        var ref2 = new Ref<T2>();
        foreach (var archetype in query.Archetypes)
        {
            var heapMap     = archetype.heapMap;
            var entityCount = archetype.EntityCount;
            var chunks1     = ((StructHeap<T1>)heapMap[query.structIndexes.T1]).chunks;
            var chunks2     = ((StructHeap<T2>)heapMap[query.structIndexes.T2]).chunks;
            var chunksLen   =  chunks1.Length;
            
            for (int chunkPos = 0; chunkPos < chunksLen; chunkPos++)
            {
                ref1.components     = chunks1[chunkPos].components;
                ref2.components     = chunks2[chunkPos].components;
                var componentLen    = entityCount;
                for (int pos = 0; pos < componentLen; pos++)
                {
                    ref1.pos = pos;
                    ref2.pos = pos;
                    lambda(ref1, ref2);
                }
            } 
        }
    }
}
