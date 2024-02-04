// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public sealed class ArchetypeQuery<T1> : ArchetypeQuery
    where T1 : struct, IComponent
{
    [Browse(Never)] internal    T1[]                copyT1;
    
    public new ArchetypeQuery<T1> AllTags       (in Tags tags) { SetHasAllTags(tags);       return this; }
    public new ArchetypeQuery<T1> AnyTags       (in Tags tags) { SetHasAnyTags(tags);       return this; }
    public new ArchetypeQuery<T1> WithoutAllTags(in Tags tags) { SetWithoutAllTags(tags);   return this; }
    public new ArchetypeQuery<T1> WithoutAnyTags(in Tags tags) { SetWithoutAnyTags(tags);   return this; }
    
    public new ArchetypeQuery<T1> AllComponents       (in ComponentTypes componentTypes) { SetHasAllComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1> AnyComponents       (in ComponentTypes componentTypes) { SetHasAnyComponents(componentTypes);       return this; }
    public new ArchetypeQuery<T1> WithoutAllComponents(in ComponentTypes componentTypes) { SetWithoutAllComponents(componentTypes);   return this; }
    public new ArchetypeQuery<T1> WithoutAnyComponents(in ComponentTypes componentTypes) { SetWithoutAnyComponents(componentTypes);   return this; }
    
    internal ArchetypeQuery(EntityStoreBase store, in Signature<T1> signature)
        : base(store, signature.signatureIndexes) {
    }
    
    public ArchetypeQuery<T1> ReadOnly<T>()
        where T : struct, IComponent
    {
        if (typeof(T1) == typeof(T)) { copyT1 = new T1[ChunkSize]; return this; }
        throw ReadOnlyException(typeof(T));
    }
    
    /// <summary> Return the <see cref="Chunk{T}"/>'s storing the components and entities of an <see cref="ArchetypeQuery{T1}"/>. </summary> 
    public      QueryChunks <T1>  Chunks                                    => new (this);
    
    [ExcludeFromCodeCoverage]
    internal QueryJob<T1> ForEach(Action<Chunk<T1>, ChunkEntities> action)  => new (this, action);
}


[ExcludeFromCodeCoverage]
internal readonly struct QueryJob<T1>
    where T1 : struct, IComponent
{
    private readonly ArchetypeQuery<T1>                 query;  //  8
    private readonly Action<Chunk<T1>, ChunkEntities>   action; //  8
    
    internal QueryJob(ArchetypeQuery<T1> query, Action<Chunk<T1>, ChunkEntities> action) {
        this.query  = query;
        this.action = action;
    }
    
    internal void Run()
    {
        foreach (Chunks<T1> chunk in query.Chunks) {
            action(chunk.Chunk1, chunk.Entities);
        }
    }
    
    internal void RunParallel()
    {
        var             localAction = action;
        WaitHandle[]    finished    = null;
        var             threadCount = Environment.ProcessorCount;
        
        foreach (Chunks<T1> chunk in query.Chunks)
        {
            if (chunk.Length < 100) {
                localAction(chunk.Chunk1, chunk.Entities);
                continue;
            }
            var step    = chunk.Length / threadCount;
            finished  ??= new WaitHandle[threadCount];   // todo pool array
            
            for (int n = 0; n < threadCount; n++)
            {
                var chunk1          = new Chunk<T1>(chunk.Chunk1,       n * step, 42);
                var entities        = new ChunkEntities(chunk.Entities, n * step, 42);

                var thread = EngineThreadPool.Instance.Execute(() => {
                    localAction(chunk1, entities);
                });
                finished[n] = thread.finished;
            }
            WaitHandle.WaitAll(finished);
        }
    }
}