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
    private  readonly   Thread              thread;
    private  readonly   EngineThreadPool    pool;
    internal readonly   AutoResetEvent      finished;

    public   override   string              ToString() => thread.Name;

    internal EngineWorker(EngineThreadPool pool, int id) {
        this.pool   = pool;
        thread      = new Thread(Run) {
            Name = $"{nameof(EngineWorker)} {id}"
        };
    }
    
    private void Run()
    {
        var poolStack = pool.stack;
        lock (poolStack) {
            poolStack.Push(this);
        }
        pool.availableThreads.Release();
    }
}


internal sealed class EngineThreadPool
{
    internal static readonly EngineThreadPool   Instance = new();
    
    internal readonly   Stack<EngineWorker>     stack;
    internal readonly   Semaphore               availableThreads;
    private             int                     threadSeq;
    
    private EngineThreadPool()
    {
        stack               = new Stack<EngineWorker>();
        availableThreads    = new Semaphore(0, Environment.ProcessorCount, "available engine threads");
    }
    
    internal EngineWorker Execute(Action action)
    {
        EngineWorker engineWorker;
        availableThreads.WaitOne();
        var poolStack = stack;
        lock (poolStack)
        {
            if (!poolStack.TryPop(out engineWorker)) {
                engineWorker = new EngineWorker(this, ++threadSeq);
            }
        }
        // engineThread.Run(action)
        return engineWorker;
    }
}

