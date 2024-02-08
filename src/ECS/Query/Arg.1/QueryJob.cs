// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable CoVariantArrayConversion
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal sealed class QueryJob<T1> : QueryJob
    where T1 : struct, IComponent
{
    internal            QueryChunks<T1>                     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryJobString();

    private readonly    ArchetypeQuery<T1>                  query;                  //  8
    private readonly    Action<Chunk<T1>, ChunkEntities>    action;                 //  8
    private             QueryJobTask[]                      jobTasks;               //  8


    private class QueryJobTask : JobTask {
        internal    Action<Chunk<T1>, ChunkEntities>    action;
        internal    Chunk<T1>                           chunk1;
        internal    ChunkEntities                       entities;
        
        internal  override void ExecuteTask()  => action(chunk1, entities);
    }
    
    internal QueryJob(ArchetypeQuery<T1> query, Action<Chunk<T1>, ChunkEntities> action) {
        this.query  = query;
        this.action = action;
        jobRunner   = query.Store.JobRunner;
    }
    
    internal override void Run()
    {
        foreach (Chunks<T1> chunk in query.Chunks) {
            action(chunk.Chunk1, chunk.Entities);
        }
    }
    
    internal override void RunParallel()
    {
        if (jobRunner == null) throw JobRunnerIsNullException();
        var taskCount   = jobRunner.workerCount + 1;
        var align512    = ComponentType<T1>.Align512;
        
        foreach (Chunks<T1> chunk in query.Chunks)
        {
            var length = chunk.Length;
            if (taskCount <= 1 || length < minParallel) {
                action(chunk.Chunk1, chunk.Entities);
                continue;
            }
            if (jobTasks == null || jobTasks.Length < taskCount) {
                jobTasks = new QueryJobTask[taskCount];
                for (int n = 0; n < taskCount; n++) {
                    jobTasks[n] = new QueryJobTask { action = action };
                }
            }
            var sectionSize = GetSectionSize(length, taskCount, align512);
            var taskIndex   = 0;
            for (int start = 0; start < length; start += sectionSize)
            {
                var task        = jobTasks[taskIndex++];
                var remaining   = length - start;
                var isLastTask  = remaining < sectionSize;
                if (isLastTask) {
                    sectionSize = remaining;
                }
                task.chunk1     = new Chunk<T1>    (chunk.Chunk1,   start, sectionSize);
                task.entities   = new ChunkEntities(chunk.Entities, start, sectionSize);
                if (isLastTask) break;
            }
            for (;taskIndex < taskCount; taskIndex++) {
                jobTasks[taskIndex] = default;
            }
            jobRunner.ExecuteJob(this, jobTasks);
        }
    }
}
