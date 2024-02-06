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
    internal readonly   ManualResetEventSlim    startWorkers        = new (false);
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
    
    internal void ExecuteJob(JobTask[] tasks, JobTask task0)
    {
        if (!workersStarted) {
            StartWorkers();
        }
        jobTasks = tasks;
        
        startWorkers.Set(); // all worker threads start running ...
            
        task0.Execute();
            
        allWorkersFinished.Wait(); // ...
        // at this point all workers are now spinning at allFinishedBarrier loop
        
        allWorkersFinished.Reset();
        
        if (startWorkers.IsSet) throw new InvalidOperationException("startWorkers.IsSet");
        
        Volatile.Write(ref finishedWorkerCount, 0);
        
        // if (n < count - 1) {
        //     startWorkers.Set(); // set before increment allFinishedBarrier to prevent blocking worker thread
        // }
        
        Interlocked.Increment(ref allFinishedBarrier);
        
        jobTasks = null;
    }
}

internal class ParallelJobWorker
{
    private readonly    ParallelJobRunner   jobRunner;
    private readonly    int                 index;

    
    internal ParallelJobWorker(ParallelJobRunner runner, int index) {
        jobRunner   = runner;
        this.index  = index;
    }
    
    internal void Run()
    {
        var runner  = jobRunner;
        var barrier = 0;
        
        while (true)
        {
            runner.startWorkers.Wait(); // wait until a task is scheduled ...
                
            var task = runner.jobTasks[index];
            task.Execute();
                
            var count = Interlocked.Increment(ref runner.finishedWorkerCount);
                
            if (count > runner.workerCount) throw new InvalidOperationException($"unexpected count: {count}");
                
            if (count == runner.workerCount)
            {
                runner.startWorkers.Reset();
                runner.allWorkersFinished.Set();
            }
            // spin wait for event to prevent preempting thread execution on: startWorkers.Wait()
            while (barrier == Volatile.Read(ref runner.allFinishedBarrier)) { }
            
            barrier++;
        }
    }
}










