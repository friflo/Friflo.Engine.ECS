// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


[ExcludeFromCodeCoverage]
internal sealed class QueryJob<T1> : QueryJob
    where T1 : struct, IComponent
{
    internal            QueryChunks<T1>                     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryChunksString();

    private readonly    ArchetypeQuery<T1>                  query;                  //  8
    private readonly    Action<Chunk<T1>, ChunkEntities>    action;                 //  8
    private             QueryJobTask[]                      jobTasks;               //  8


    private class QueryJobTask : JobTask {
        internal    Action<Chunk<T1>, ChunkEntities>    action;
        internal    Chunk<T1>                           chunk1;
        internal    ChunkEntities                       entities;
        
        internal  override void Execute()  => action(chunk1, entities);
    }
    
    internal QueryJob(ArchetypeQuery<T1> query, Action<Chunk<T1>, ChunkEntities> action) {
        this.query              = query;
        this.action             = action;
    }

    internal override void Run()
    {
        foreach (Chunks<T1> chunk in query.Chunks) {
            action(chunk.Chunk1, chunk.Entities);
        }
    }
    
    internal override void RunParallel()
    {
        var taskCount   = jobRunner.workerCount + 1;
        
        foreach (Chunks<T1> chunk in query.Chunks)
        {
            if (taskCount <= 1 || chunk.Length < minParallel) {
                action(chunk.Chunk1, chunk.Entities);
                continue;
            }
            var step = chunk.Length / taskCount;
            if (jobTasks == null) {
                jobTasks = new QueryJobTask[taskCount];
                for (int n = 0; n < taskCount; n++) {
                    jobTasks[n] = new QueryJobTask { action = action };
                }
            }
            for (int n = 0; n < taskCount; n++)
            {
                var start       = n * step;
                var task        = jobTasks[n];
                task.chunk1     = new Chunk<T1>(chunk.Chunk1,       start, step);
                task.entities   = new ChunkEntities(chunk.Entities, start, step);
                if (n < taskCount - 1) {
                    continue;
                }
                // --- last job task
                // ReSharper disable once CoVariantArrayConversion
                jobRunner.ExecuteJob(jobTasks);
                break;
            }
        }
    }
}
