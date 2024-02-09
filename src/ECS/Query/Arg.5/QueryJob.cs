// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable CoVariantArrayConversion
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public sealed class QueryJob<T1, T2, T3, T4, T5> : QueryJob
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
{
    internal            QueryChunks<T1, T2, T3, T4, T5>     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryJobString();

    private readonly    ArchetypeQuery<T1, T2, T3, T4, T5>                                              query;      //  8
    private readonly    Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, Chunk<T4>, Chunk<T5>, ChunkEntities>    action;     //  8
    private             QueryJobTask[]                                                                  jobTasks;   //  8


    private class QueryJobTask : JobTask {
        internal    Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, Chunk<T4>, Chunk<T5>, ChunkEntities>    action;
        internal    Chunks<T1, T2, T3, T4, T5>                                                      chunks;
        
        internal  override void ExecuteTask()  => action(chunks.Chunk1, chunks.Chunk2, chunks.Chunk3, chunks.Chunk4, chunks.Chunk5, chunks.Entities);
    }
    
    internal QueryJob(
        ArchetypeQuery<T1, T2, T3, T4, T5>                                              query,
        Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, Chunk<T4>, Chunk<T5>, ChunkEntities>    action)
    {
        this.query  = query;
        this.action = action;
        jobRunner   = query.Store.JobRunner;
    }
    
    public override void Run()
    {
        foreach (Chunks<T1, T2, T3, T4, T5> chunk in query.Chunks) {
            action(chunk.Chunk1, chunk.Chunk2, chunk.Chunk3, chunk.Chunk4, chunk.Chunk5, chunk.Entities);
        }
    }
    
    public override void RunParallel()
    {
        if (jobRunner == null) throw JobRunnerIsNullException();
        var taskCount   = jobRunner.workerCount + 1;
        
        foreach (Chunks<T1, T2, T3, T4, T5> chunk in query.Chunks)
        {
            var chunkLength = chunk.Length;
            if (taskCount <= 1 || chunkLength < minParallel) {
                action(chunk.Chunk1, chunk.Chunk2, chunk.Chunk3, chunk.Chunk4, chunk.Chunk5, chunk.Entities);
                continue;
            }
            var tasks = jobTasks;
            if (tasks == null || tasks.Length < taskCount) {
                tasks = jobTasks = new QueryJobTask[taskCount];
                for (int n = 0; n < taskCount; n++) {
                    tasks[n] = new QueryJobTask { action = action };
                }
            }
            var sectionSize = GetSectionSize512(chunkLength, taskCount, Align512);
            var start       = 0;
            for (int taskIndex = 0; taskIndex < taskCount; taskIndex++)
            {
                var length      = GetSectionLength (chunkLength,    start, sectionSize);
                var chunk1      = new Chunk<T1>    (chunk.Chunk1,   start, length);
                var chunk2      = new Chunk<T2>    (chunk.Chunk2,   start, length);
                var chunk3      = new Chunk<T3>    (chunk.Chunk3,   start, length);
                var chunk4      = new Chunk<T4>    (chunk.Chunk4,   start, length);
                var chunk5      = new Chunk<T5>    (chunk.Chunk5,   start, length);
                var entities    = new ChunkEntities(chunk.Entities, start, length, taskIndex);
                tasks[taskIndex].chunks = new Chunks<T1, T2, T3, T4, T5>(chunk1, chunk2, chunk3, chunk4, chunk5, entities);
                start          += sectionSize;
            }
            jobRunner.ExecuteJob(this, tasks);
        }
    }
    
    private static readonly int Align512 = GetAlign512();
    
    private static int GetAlign512()
    {
        int lcm1 = ComponentType<T1>.Align512;
        int lcm2 = ComponentType<T2>.Align512;
        int lcm3 = ComponentType<T3>.Align512;
        int lcm4 = ComponentType<T4>.Align512;
        int lcm5 = ComponentType<T5>.Align512;
        int lcm12   =   LeastCommonMultiple(lcm1,    lcm2);
        int lcm34   =   LeastCommonMultiple(lcm3,    lcm4);
        int lcm1234 =   LeastCommonMultiple(lcm12,   lcm34);
        return          LeastCommonMultiple(lcm1234, lcm5);
    }
}
