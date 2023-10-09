// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

#region --- T1 T2

public readonly struct QueryForEach<T1, T2>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    private readonly    ArchetypeQuery<T1,T2>       query;
    private readonly    Action<Ref<T1>, Ref<T2>>    lambda;
    private readonly    T1[]                        copyT1;
    private readonly    T2[]                        copyT2;

    public  override    string                      ToString() => query.signature.types.GetString("ForEach: ");

    internal QueryForEach(
        ArchetypeQuery<T1, T2>      query,
        Action<Ref<T1>, Ref<T2>>    lambda)
    {
        this.query  = query;
        this.lambda = lambda;
        copyT1      = query.copyT1;
        copyT2      = query.copyT2;
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
                ref1.Set(chunks1[chunkPos].components, copyT1, componentLen);
                ref2.Set(chunks2[chunkPos].components, copyT2, componentLen);
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
