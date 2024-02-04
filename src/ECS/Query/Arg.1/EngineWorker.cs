// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class EngineWorker
{
    private  readonly   string              name;
    private  readonly   EngineWorkerPool    pool;
    private  readonly   AutoResetEvent      start;
    internal readonly   AutoResetEvent      finished;
    private             Action              action;
    private             bool                running;

    public   override   string              ToString() => GetString();

    internal EngineWorker(EngineWorkerPool pool, int id) {
        this.pool   = pool;
        start       = new AutoResetEvent(false); // false: Not signaled
        finished    = new AutoResetEvent(false); // false: Not signaled
        name        = $"{nameof(EngineWorker)} {id}";
         var thread = new Thread(Run) {
            IsBackground    = true,
            Name            = name
        };
        thread.Start();
    }
    
    internal void Signal(Action action)
    {
        this.action = action;
        start.Set(); // Sets the state of the event to signaled, allowing one or more waiting threads to proceed.
    }
    
    private void Run()
    {
        while (true)
        {
            try {
                start.WaitOne();
                running = true;
                // ReSharper disable once PossibleNullReferenceException - waiting on finished ensures action is not null
                action();
            }
            finally {
                finished.Set();
                running = false;
                action  = null;
                var poolStack = pool.stack;
                lock (poolStack) {
                    poolStack.Push(this);
                }
                // pool.availableThreads.Release();
            }
        }
    }
    
    private string GetString()
    {
        if (action == null) {
            return name + " - idle";
        }
        if (running) {
            return name + " - running";
        }
        return name + " - waiting";
    }
}


internal sealed class EngineWorkerPool
{
    private  static readonly EngineWorkerPool   Instance = new();
    
    internal readonly   Stack<EngineWorker>     stack;
//  internal readonly   Semaphore               availableThreads;
    private             int                     threadSeq;
    
    private EngineWorkerPool()
    {
        stack               = new Stack<EngineWorker>();
    //  var count           = Environment.ProcessorCount;
    //  availableThreads    = new Semaphore(count, count, "available engine threads");
    }
    
    internal static void GetWorkers(EngineWorker[] workers, int count)
    {
        var             pool        = Instance; 
        var             poolStack   = pool.stack;
        
        // for (int n = 0; n < count; n++) { pool.availableThreads.WaitOne(); }
        lock (poolStack)
        {
            for (int n = 0; n < count; n++) {
                if (!poolStack.TryPop(out var worker)) {
                    worker = new EngineWorker(pool, ++pool.threadSeq);
                }
                workers[n] = worker; 
            }
        }
    }
    
    /* internal static void ReturnWorkers(EngineWorker[] workers, int count)
    {
        var             pool        = Instance; 
        for (int n = 0; n < count; n++) { pool.availableThreads.Release(); }
    } */
}

