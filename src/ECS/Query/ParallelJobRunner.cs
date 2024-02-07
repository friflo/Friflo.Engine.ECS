// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class JobTask {
    internal abstract void Execute();
}

/// <remarks>
/// The main goals of the <see cref="ParallelJobRunner"/>:<br/>
/// - Minimize calls to synchronization primitives.<br/>
/// - Use cheap synchronization primitives such as:
///   <see cref="ManualResetEventSlim"/>, <see cref="Interlocked"/> and <see cref="Volatile"/>.<br/>
/// - Minimize thread context switches caused by <see cref="ManualResetEventSlim"/> in case calling
///   <see cref="ManualResetEventSlim.Wait()"/> when the event is not signaled.<br/>
/// <br/>
/// Use analyze amount of thread context switches use: Process Explorer > Column > CSwitch Delta.<br/>
/// </remarks>
internal sealed class ParallelJobRunner
{
#region fields
    private  readonly   ManualResetEventSlim    startWorkers        = new (false);
    private  readonly   ManualResetEventSlim    allWorkersFinished  = new (false);
    private             int                     allFinishedBarrier;
    private             int                     finishedWorkerCount;
    private             bool                    workersStarted;
    private             JobTask[]               jobTasks;
    internal readonly   int                     workerCount;
    
    internal static readonly ParallelJobRunner Default = new ParallelJobRunner(Environment.ProcessorCount);
    #endregion
    
    internal ParallelJobRunner(int threadCount) {
        workerCount = threadCount - 1;
    }
    
    private void StartWorkers()
    {
        workersStarted = true;
        for (int index = 0; index < workerCount; index++)
        {
            var worker = new ParallelJobWorker(index + 1);
            var thread = new Thread(() => RunWorker(worker)) {
                Name            = $"ParallelJobWorker - {index}",
                IsBackground    = true
            };
            thread.Start();
        }
    }
    
    // ------------------------------------------ job thread -------------------------------------------
    internal void ExecuteJob(JobTask[] tasks)
    {
        if (!workersStarted) {
            StartWorkers();
        }
        jobTasks = tasks;
        
        Volatile.Write(ref finishedWorkerCount, 0);
        
        // set before increment allFinishedBarrier to prevent blocking worker thread
        startWorkers.Set(); // all worker threads start running ...

        Interlocked.Increment(ref allFinishedBarrier);

        tasks[0].Execute();
            
        allWorkersFinished.Wait();

        allWorkersFinished.Reset();
        
        if (startWorkers.IsSet) throw new InvalidOperationException("startWorkers.IsSet");
        
        jobTasks = null;
    }
    
    // ----------------------------------------- worker thread -----------------------------------------
    private void RunWorker(ParallelJobWorker worker)
    {
        
        var barrier = 0;
        var index   = worker.index;
        while (true)
        {
            // --- wait until a task is scheduled ...
            // spin wait for event to prevent preempting thread execution on: startWorkers.Wait()
            while (barrier == Volatile.Read(ref allFinishedBarrier)) {
                Thread.SpinWait(30);
            }
            barrier++;            

            startWorkers.Wait();
            
            // --- execute task
            var task = jobTasks[index];
            task.Execute();
                
            // ---
            var count = Interlocked.Increment(ref finishedWorkerCount);
            if (count > workerCount) throw new InvalidOperationException($"unexpected count: {count}");
            if (count == workerCount)
            {
                startWorkers.Reset();
                allWorkersFinished.Set();
            }
        }
    }
}

internal sealed class ParallelJobWorker
{
    internal readonly    int                 index;
    
    internal ParallelJobWorker(int index) {
        this.index  = index;
    }
}










