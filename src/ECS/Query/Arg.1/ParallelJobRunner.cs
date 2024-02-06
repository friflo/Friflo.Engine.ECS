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

internal class ParallelJobRunner
{
    internal readonly   ManualResetEventSlim    startWorkers        = new (false, 2047);
    internal readonly   ManualResetEventSlim    allWorkersFinished  = new (false);
    internal            int                     allFinishedBarrier;
    internal            int                     finishedWorkerCount;
    private             bool                    workersStarted;
    internal            JobTask[]               jobTasks;
    internal readonly   int                     workerCount;
    
    
    internal ParallelJobRunner(int threadCount) {
        workerCount = threadCount - 1;
    }
    
    private void StartWorkers()
    {
        workersStarted = true;
        for (int index = 0; index < workerCount; index++)
        {
            var worker = new ParallelJobWorker(this, index);
            var thread = new Thread(() => worker.Run()) {
                Name            = $"ParallelJobWorker - {index}",
                IsBackground    = true
            };
            thread.Start();
        }
    }
    
    // ------------------------------------------ job thread -------------------------------------------
    internal void ExecuteJob(JobTask[] tasks, JobTask task0)
    {
        if (!workersStarted) {
            StartWorkers();
        }
        jobTasks = tasks;
        
        Volatile.Write(ref finishedWorkerCount, 0);
        
        // set before increment allFinishedBarrier to prevent blocking worker thread
        startWorkers.Set(); // all worker threads start running ...

        Interlocked.Increment(ref allFinishedBarrier);

        task0.Execute();
            
        allWorkersFinished.Wait();

        allWorkersFinished.Reset();
        
        if (startWorkers.IsSet) throw new InvalidOperationException("startWorkers.IsSet");
        
        jobTasks = null;
    }
}

internal class ParallelJobWorker
{
    // ----------------------------------------- worker thread -----------------------------------------
    internal void Run()
    {
        var runner          = jobRunner;
        var barrier         = 0;
        
        while (true)
        {
            // --- wait until a task is scheduled ...
            // spin wait for event to prevent preempting thread execution on: startWorkers.Wait()
            while (barrier == Volatile.Read(ref runner.allFinishedBarrier)) { Thread.SpinWait(1);  }
            barrier++;

            runner.startWorkers.Wait();
            
            // --- execute task
            var task = runner.jobTasks[index];
            task.Execute();
                
            // ---
            var count = Interlocked.Increment(ref runner.finishedWorkerCount);
            if (count > runner.workerCount) throw new InvalidOperationException($"unexpected count: {count}");
            if (count == runner.workerCount)
            {
                runner.startWorkers.Reset();
                runner.allWorkersFinished.Set();
            }
        }
    }
    
    private readonly    ParallelJobRunner   jobRunner;
    private readonly    int                 index;
    
    internal ParallelJobWorker(ParallelJobRunner runner, int index) {
        jobRunner   = runner;
        this.index  = index;
    }
    
    #region test
    /* private const int SpinMax = 5000;

    private void WaitStartWorkers()
    {
        var startWorkers = jobRunner.startWorkers;
        for (int n = 0; n < SpinMax; n++)
        {
            if (startWorkers.IsSet) {
                ++passCount;
                return;
            }
        }
        startWorkers.Wait();
        if (++waitCount % 100 == 0) Console.WriteLine($"passCount: {passCount},  waitCount: {waitCount}");
    }

    int passCount;
    int waitCount;
    long spinWaitCount; */
    #endregion
}










