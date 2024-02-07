// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal abstract class JobTask {
    internal abstract void ExecuteTask();
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
internal sealed class ParallelJobRunner : IDisposable
{
#region
    public              bool                    IsDisposed  => !running;
    public              int                     ThreadCount => workerCount + 1;
    public override     string                  ToString()  => $"{name} - threads: {ThreadCount}";
    #endregion
    
#region fields
    private  readonly   ManualResetEventSlim    startWorkers        = new (false, 2047);
    private  readonly   ManualResetEventSlim    allWorkersFinished  = new (false);
    private  readonly   List<Exception>         taskExceptions      = new ();
    private             int                     allFinishedBarrier;
    private             int                     finishedWorkerCount;
    private             bool                    workersStarted;
    private             JobTask[]               jobTasks;
    internal readonly   int                     workerCount;
    private             bool                    running;
    private             object                  inUseByJob;
    private  readonly   string                  name;
    
    private  static     int     _jobRunnerIdSeq;
    #endregion
    
#region general
    internal ParallelJobRunner(int threadCount, string name = null) {
        workerCount = threadCount - 1;
        running     = true;
        if (name != null) {
            this.name = name;
            return;
        }
        this.name = "JobRunner " + ++_jobRunnerIdSeq;
    }

    public void Dispose() {
        running     = false;
        startWorkers.Set();
    }

    private void StartWorkers()
    {
        workersStarted = true;
        for (int index = 1; index <= workerCount; index++)
        {
            var thread = new Thread(RunWorker) {
                Name            = $"{name} - worker {index}",
                IsBackground    = true
            };
            thread.Start(index);
        }
    }
    
    private void AddTaskException (Exception exception)
    {
        lock (taskExceptions) {
            taskExceptions.Add(exception);
        }
    }
    
    private static ObjectDisposedException RunnerDisposedException ()
    {
        return new ObjectDisposedException(nameof(ParallelJobRunner));
    }
    
    private InvalidOperationException AlreadyInUseException (object job)
    {
        return new InvalidOperationException($"{nameof(ParallelJobRunner)} ({name}) is already in use by: {job}");
    }
    
    private AggregateException JobException (object job)
    {
        return new AggregateException($"{job} - {taskExceptions.Count} task exceptions.", taskExceptions);
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    private void AssertStartWorkersNotSignaled() {
        if (startWorkers.IsSet) throw new InvalidOperationException("startWorkers.IsSet");
    }
    
    [Conditional("DEBUG")] [ExcludeFromCodeCoverage]
    private void AssertWorkerCount(int count) {
        if (count > workerCount) throw new InvalidOperationException($"unexpected count: {count}");
    }
    #endregion
    
    // ----------------------------------- job on caller thread -----------------------------------
    internal void ExecuteJob(object job, JobTask[] tasks)
    {
        if (inUseByJob != null) throw AlreadyInUseException(inUseByJob);
        if (!running)           throw RunnerDisposedException();
        taskExceptions.Clear();
        if (!workersStarted) {
            StartWorkers();
        }
        inUseByJob  = job;
        jobTasks    = tasks;
        
        Volatile.Write(ref finishedWorkerCount, 0);
        startWorkers.Set(); // all worker threads start running ...
        
        try {
            tasks[0].ExecuteTask();
        } catch (Exception exception) {
            AddTaskException(exception);
        }
        allWorkersFinished.Wait();

        allWorkersFinished.Reset();
        
        AssertStartWorkersNotSignaled();
        
        Interlocked.Increment(ref allFinishedBarrier);
        
        jobTasks = null;
        if (taskExceptions.Count > 0) {
            throw JobException(job);
        }
        inUseByJob = null;
    }
    
    // ------------------------------------ worker thread loop ------------------------------------
    private void RunWorker(object workerIndex)
    {
        var barrier = 0;
        var index   = (int)workerIndex;
        while (running)
        {
            startWorkers.Wait();
            if (!running) break;
            
            // --- execute task
            try {
                jobTasks[index].ExecuteTask();
            } catch (Exception exception) {
                AddTaskException(exception);
            }
            // ---
            var count = Interlocked.Increment(ref finishedWorkerCount);
            AssertWorkerCount(count);
            if (count == workerCount)
            {
                startWorkers.Reset();
                allWorkersFinished.Set();
            }
            // Spin wait for all workers are finished their task.
            // Required to ensure startWorkers is not signal when they reach startWorkers.Wait() above.
            while (barrier == Volatile.Read(ref allFinishedBarrier)) {
                // Thread.SpinWait(30);
            }
            barrier++;
        }
    }
}
