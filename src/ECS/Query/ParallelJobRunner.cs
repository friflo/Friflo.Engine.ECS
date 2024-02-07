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
internal sealed class ParallelJobRunner : IDisposable
{
#region fields
    private  readonly   ManualResetEventSlim    startWorkers        = new (false, 2047);
    private  readonly   ManualResetEventSlim    allWorkersFinished  = new (false);
    private  readonly   object                  monitor             = new object();
    private  readonly   List<Exception>         taskExceptions      = new ();
    private             int                     allFinishedBarrier;
    private             int                     finishedWorkerCount;
    private             bool                    workersStarted;
    private             JobTask[]               jobTasks;
    internal readonly   int                     workerCount;
    private             bool                    running;
    
    internal static readonly ParallelJobRunner Default = new ParallelJobRunner(Environment.ProcessorCount);
    #endregion
    
#region general
    internal ParallelJobRunner(int threadCount) {
        workerCount = threadCount - 1;
        running     = true;
    }

    public void Dispose() {
        running     = false;
        startWorkers.Set();
    }

    private void StartWorkers()
    {
        workersStarted = true;
        for (int index = 0; index < workerCount; index++)
        {
            var thread = new Thread(RunWorker) {
                Name            = $"ParallelJobWorker - {index}",
                IsBackground    = true
            };
            var worker = new ParallelJobWorker(index + 1);
            thread.Start(worker);
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
        lock (monitor)
        {
            if (!running) throw RunnerDisposedException();
            taskExceptions.Clear();
            if (!workersStarted) {
                StartWorkers();
            }
            jobTasks = tasks;
            
            Volatile.Write(ref finishedWorkerCount, 0);
            startWorkers.Set(); // all worker threads start running ...
            
            try {
                tasks[0].Execute();
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
        }
    }
    
    // ------------------------------------ worker thread loop ------------------------------------
    private void RunWorker(object worker)
    {
        var barrier = 0;
        var index   = ((ParallelJobWorker)worker).index;
        while (running)
        {
            startWorkers.Wait();
            if (!running) break;
            
            // --- execute task
            var task = jobTasks[index];
            try {
                task.Execute();
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

internal sealed class ParallelJobWorker
{
    internal readonly    int                 index;
    
    internal ParallelJobWorker(int index) {
        this.index  = index;
    }
}
