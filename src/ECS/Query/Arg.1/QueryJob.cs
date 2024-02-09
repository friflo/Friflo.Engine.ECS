// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable CoVariantArrayConversion
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public sealed class QueryJob<T1> : QueryJob
    where T1 : struct, IComponent
{
    internal            QueryChunks<T1>                     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryJobString();

    private readonly    ArchetypeQuery<T1>                  query;                  //  8
    private readonly    Action<Chunk<T1>, ChunkEntities>    action;                 //  8
    private             QueryJobTask[]                      jobTasks;               //  8


    private class QueryJobTask : JobTask {
        internal    Action<Chunk<T1>, ChunkEntities>    action;
        internal    Chunks<T1>                          chunks;
        
        internal  override void ExecuteTask()  => action(chunks.Chunk1, chunks.Entities);
    }
    
    internal QueryJob(
        ArchetypeQuery<T1>                  query,
        Action<Chunk<T1>, ChunkEntities>    action)
    {
        this.query  = query;
        this.action = action;
        jobRunner   = query.Store.JobRunner;
    }
    
    public override void Run()
    {
        foreach (Chunks<T1> chunk in query.Chunks) {
            action(chunk.Chunk1, chunk.Entities);
        }
    }
    
    public override void RunParallel()
    {
        if (jobRunner == null) throw JobRunnerIsNullException();
        var taskCount   = jobRunner.workerCount + 1;
        
        foreach (Chunks<T1> chunk in query.Chunks)
        {
            var chunkLength = chunk.Length;
            if (taskCount <= 1 || chunkLength < minParallel) {
                action(chunk.Chunk1, chunk.Entities);
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
                var entities    = new ChunkEntities(chunk.Entities, start, length, taskIndex);
                tasks[taskIndex].chunks = new Chunks<T1>(chunk1, entities);
                start          += sectionSize;
            }
            jobRunner.ExecuteJob(this, tasks);
        }
    }
    
    private static readonly int Align512 = ComponentType<T1>.Align512;
}
