﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable CoVariantArrayConversion
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Enables <see cref="JobExecution.Parallel"/> query execution returning the specified components.
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#parallel-query-job">Example.</a>
/// </summary>
public sealed class QueryJob<T1, T2, T3, T4, T5> : QueryJob
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
{
    internal            QueryChunks<T1, T2, T3, T4, T5>     Chunks      => new (query);     // only for debugger
    internal            QueryEntities                       Entities    => query.Entities;  // only for debugger
    public  override    string                              ToString()  => query.GetQueryJobString();

    [Browse(Never)]
    private readonly    ArchetypeQuery<T1, T2, T3, T4, T5>                                              query;      //  8
    private readonly    Action<Chunk<T1>, Chunk<T2>, Chunk<T3>, Chunk<T4>, Chunk<T5>, ChunkEntities>    action;     //  8
    [Browse(Never)]
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
    
    /// <summary>Execute the query.
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#parallel-query-job">Example.</a>.<br/>
    /// All chunks having at least <see cref="QueryJob.MinParallelChunkLength"/> * <see cref="ParallelJobRunner.ThreadCount"/>
    /// components are executed <see cref="JobExecution.Parallel"/>. 
    /// </summary>
    public override void RunParallel()
    {
        if (jobRunner == null) throw JobRunnerIsNullException();
        var taskCount   = jobRunner.workerCount + 1;
        
        foreach (Chunks<T1, T2, T3, T4, T5> chunks in query.Chunks)
        {
            var chunkLength = chunks.Length;
            if (ExecuteSequential(taskCount, chunkLength)) {
                action(chunks.Chunk1, chunks.Chunk2, chunks.Chunk3, chunks.Chunk4, chunks.Chunk5, chunks.Entities);
                continue;
            }
            var tasks = jobTasks;
            if (tasks == null || tasks.Length < taskCount) {
                tasks = jobTasks = new QueryJobTask[taskCount];
                for (int n = 0; n < taskCount; n++) {
                    tasks[n] = new QueryJobTask { action = action };
                }
            }
            var sectionSize = GetSectionSize(chunkLength, taskCount, Multiple);
            var start       = 0;
            for (int taskIndex = 0; taskIndex < taskCount; taskIndex++)
            {
                var length = GetSectionLength (chunkLength, start, sectionSize);
                if (length > 0) {
                    tasks[taskIndex].chunks = new Chunks<T1, T2, T3, T4, T5>(chunks, start, length, taskIndex);
                    start += sectionSize;
                    continue;
                }
                for (; taskIndex < taskCount; taskIndex++) {
                    tasks[taskIndex].chunks = new Chunks<T1, T2, T3, T4, T5>(chunks.Entities, taskIndex);
                }
                break;
            }
            jobRunner.ExecuteJob(this, tasks);
        }
    }
    
    public  override        int ParallelComponentMultiple   => Multiple;
    private static readonly int Multiple                    = GetMultiple();
    
    private static int GetMultiple()
    {
        int lcm1 = StructPadding<T1>.ComponentMultiple;
        int lcm2 = StructPadding<T2>.ComponentMultiple;
        int lcm3 = StructPadding<T3>.ComponentMultiple;
        int lcm4 = StructPadding<T4>.ComponentMultiple;
        int lcm5 = StructPadding<T5>.ComponentMultiple;
        int lcm12   =   LeastComponentMultiple(lcm1,    lcm2);
        int lcm34   =   LeastComponentMultiple(lcm3,    lcm4);
        int lcm1234 =   LeastComponentMultiple(lcm12,   lcm34);
        return          LeastComponentMultiple(lcm1234, lcm5);
    }
}
