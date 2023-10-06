// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

#region T1 T2

public struct ForEachQuery<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private readonly    ArchetypeQuery<T1,T2>      query;
    private readonly    Action<Ref<T1>, Ref<T2>>   lambda;
    private             T1[]                       copyT1;
    private             T2[]                       copyT2;
    
    internal ForEachQuery(
        ArchetypeQuery<T1, T2>      query,
        Action<Ref<T1>, Ref<T2>>    lambda)
    {
        this.query  = query;
        this.lambda = lambda;
        copyT1      = query.readOnlyT1 ? new T1[10] : null;
        copyT2      = query.readOnlyT2 ? new T2[10] : null;
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
                var componentLen    = entityCount;
                ref1.Set(chunks1[chunkPos].components, ref copyT1, componentLen);
                ref2.Set(chunks2[chunkPos].components, ref copyT2, componentLen);
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
#endregion
