// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal class QueryJob<T1>
    where T1 : struct, IComponent
{
    internal            QueryChunks<T1>                     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryChunksString();
    
    public              int                                 ThreadCount;            //  4
    public              int                                 MinParallelChunkLength; //  4
    
    private readonly    ArchetypeQuery<T1>                  query;                  //  8
    private readonly    Action<Chunk<T1>, ChunkEntities>    action;                 //  8
    private             JobAction[]                         jobActions;             //  8
    private             WaitHandle[]                        finished;
    private             EngineWorker[]                      workers;

    
    private class JobAction : WorkerAction {
        internal    Action<Chunk<T1>, ChunkEntities>    action;
        internal    Chunk<T1>                           chunk1;
        internal    ChunkEntities                       entities;
        
        internal  override void Execute() => action(chunk1, entities);
    }
    
    internal QueryJob(ArchetypeQuery<T1> query, Action<Chunk<T1>, ChunkEntities> action) {
        this.query              = query;
        this.action             = action;
        ThreadCount             = Environment.ProcessorCount;
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
        var threadCount = ThreadCount;
        
        foreach (Chunks<T1> chunk in query.Chunks)
        {
            if (threadCount <= 1 || chunk.Length < MinParallelChunkLength) {
                localAction(chunk.Chunk1, chunk.Entities);
                continue;
            }
            var step    = chunk.Length / threadCount;
            finished  ??= new WaitHandle  [threadCount - 1];    // todo pool array
            workers   ??= new EngineWorker[threadCount - 1];    // todo pool array
            if (jobActions == null) {
                jobActions  = new JobAction[threadCount];       // todo pool array
                for (int n = 0; n < threadCount; n++) {
                    jobActions[n] = new JobAction { action = localAction };
                }
            }
            EngineWorkerPool.GetWorkers(workers, threadCount - 1);
            
            for (int n = 0; n < threadCount; n++)
            {
                var start       = n * step;
                var length      = chunk.Length / threadCount;
                var job         = jobActions[n];
                job.chunk1      = new Chunk<T1>(chunk.Chunk1,       start, length);
                job.entities    = new ChunkEntities(chunk.Entities, start, length);
                if (n < threadCount - 1) {
                    var worker      = workers[n];
                    finished[n] = worker.finished;
                    worker.Signal(job);
                    continue;
                }
                // --- last job task
                job.Execute();
                break;
            }
            WaitHandle.WaitAll(finished);
        }
    }
}
