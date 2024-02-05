// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal struct QueryJob<T1>
    where T1 : struct, IComponent
{
    internal            QueryChunks<T1>                     Chunks      => new (query); // only for debugger
    public  override    string                              ToString()  => query.GetQueryChunksString();
    
    public              int                                 ThreadCount;            //  4
    public              int                                 MinParallelChunkLength; //  4
    
    private readonly    ArchetypeQuery<T1>                  query;                  //  8
    private readonly    Action<Chunk<T1>, ChunkEntities>    action;                 //  8
    private             JobAction                           jobAction;              //  8
    
    private class JobAction : WorkerAction {
        internal    Action<Chunk<T1>, ChunkEntities>    action;
        internal    Chunk<T1>                           chunk1;
        internal    ChunkEntities                       entities;
        internal    WaitHandle[]                        finished;
        internal    EngineWorker[]                      workers;
        
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
            var job     = jobAction ??= new JobAction {
                action      = localAction,
                finished    = new WaitHandle  [threadCount - 1],    // todo pool array
                workers     = new EngineWorker[threadCount - 1]     // todo pool array
            };
            EngineWorkerPool.GetWorkers(job.workers, threadCount - 1);
            
            for (int n = 0; n < threadCount; n++)
            {
                var start     = n * step;
                var length    = chunk.Length / threadCount;
                job.chunk1    = new Chunk<T1>(chunk.Chunk1,       start, length);
                job.entities  = new ChunkEntities(chunk.Entities, start, length);
                if (n < threadCount - 1) {
                    var worker      = job.workers[n];
                    job.finished[n] = worker.finished;
                    worker.Signal(job);
                    continue;
                }
                job.Execute();
                break;
            }
            WaitHandle.WaitAll(job.finished);
        }
    }
}
