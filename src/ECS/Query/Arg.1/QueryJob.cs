// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


[ExcludeFromCodeCoverage]
internal class QueryJob<T1>
    where T1 : struct, IComponent
{
    internal            QueryChunks<T1>                     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryChunksString();
    
    public              int                                 MinParallelChunkLength; //  4
    public              ParallelJobRunner                   JobRunner { get; set; } //  8
    
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
        MinParallelChunkLength  = 1000;
    }

    internal void Run()
    {
        foreach (Chunks<T1> chunk in query.Chunks) {
            action(chunk.Chunk1, chunk.Entities);
        }
    }
    
    internal void RunParallel()
    {
        var localAction = action;
        var runner      = JobRunner;
        var threadCount = runner.workerCount;
        
        foreach (Chunks<T1> chunk in query.Chunks)
        {
            if (threadCount <= 1 || chunk.Length < MinParallelChunkLength) {
                localAction(chunk.Chunk1, chunk.Entities);
                continue;
            }
            var step    = chunk.Length / threadCount;
            if (jobTasks == null) {
                jobTasks    = new QueryJobTask[threadCount];       // todo pool array
                for (int n = 0; n < threadCount; n++) {
                    jobTasks[n] = new QueryJobTask { action = localAction };
                }
            }
            for (int n = 0; n < threadCount; n++)
            {
                var start       = n * step;
                var length      = chunk.Length / threadCount;
                var task        = jobTasks[n];
                task.chunk1     = new Chunk<T1>(chunk.Chunk1,       start, length);
                task.entities   = new ChunkEntities(chunk.Entities, start, length);
                if (n < threadCount - 1) {
                    continue;
                }
                // --- last job task
                // ReSharper disable once CoVariantArrayConversion
                runner.ExecuteJob(jobTasks, task);
                break;
            }
        }
    }
}
