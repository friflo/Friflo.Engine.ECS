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
/// <see cref="ParallelJobRunner"/> is thread safe.<br/>
/// The intention is to use the same instance for all jobs. E.g. the JobRunner assigned to the <see cref="EntityStore"/>.<br/>
/// When executing nested jobs - running a job within another job - the nested job requires its own runner.<br/> 
/// <br/>
/// Performance related implementation goals:<br/>
/// - Minimize calls to synchronization primitives.<br/>
/// - Use cheap synchronization primitives such as:
///   <see cref="ManualResetEventSlim"/>, <see cref="Interlocked"/> and <see cref="Volatile"/>.<br/>
/// - Minimize thread context switches caused by <see cref="ManualResetEventSlim"/> in case calling
///   <see cref="ManualResetEventSlim.Wait()"/> when the event is not signaled.<br/>
/// Note: To analyze the amount of thread context switches use: Process Explorer > Column > CSwitch Delta.
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
    private  readonly   string                  name;                   // never null. used also as monitor 
    private             object                  usedBy;
    // --- static
    [ThreadStatic]
    private  static     Stack<ParallelJobRunner>_tlsUsedRunnerStack; // single instance per thread
    private  static     int                     _jobRunnerIdSeq;
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
    
    private InvalidOperationException AlreadyUsedByException (object job)
    {
        return new InvalidOperationException($"'{name}' is already used by <- {job}");
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
        var stack = _tlsUsedRunnerStack ??= new Stack<ParallelJobRunner>();
        foreach (var runner in stack) {
            if (runner == this) throw AlreadyUsedByException(usedBy);    
        }
        lock(name)
        {
            if (!running)       throw RunnerDisposedException();
            taskExceptions.Clear();
            if (!workersStarted) {
                StartWorkers();
            }
            jobTasks = tasks;
            usedBy   = job;
            
            Volatile.Write(ref finishedWorkerCount, 0);
            startWorkers.Set(); // all worker threads start running ...
            
            stack.Push(this);
            try {
                tasks[0].ExecuteTask();
            } catch (Exception exception) {
                AddTaskException(exception);
            }
            stack.Pop();

            allWorkersFinished.Wait();

            allWorkersFinished.Reset();
            
            AssertStartWorkersNotSignaled();
            
            Interlocked.Increment(ref allFinishedBarrier);
            
            jobTasks = null;
            usedBy   = null;
            if (taskExceptions.Count > 0) {
                throw JobException(job);
            }
        }
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
            var stack = _tlsUsedRunnerStack ??= new Stack<ParallelJobRunner>();
            stack.Push(this);
            try {
                jobTasks[index].ExecuteTask();
            } catch (Exception exception) {
                AddTaskException(exception);
            }
            stack.Pop();
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
