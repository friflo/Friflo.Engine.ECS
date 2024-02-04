// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class EngineThread
{
    private  readonly   Thread              thread;
    private  readonly   EngineThreadPool    pool;
    internal readonly   AutoResetEvent      finished;
    
    internal EngineThread(EngineThreadPool pool) {
        this.pool   = pool;
        thread      = new Thread(Run);
    }
    
    internal void Run()
    {

        pool.availableThreads.Release();
    }
}


internal sealed class EngineThreadPool
{
    internal static readonly EngineThreadPool   Instance = new();
    
    private  readonly   Stack<EngineThread>     pool;
    internal readonly   Semaphore               availableThreads;
    
    private EngineThreadPool()
    {
        pool                = new Stack<EngineThread>();
        availableThreads    = new Semaphore(0, Environment.ProcessorCount, "available engine threads");
    }
    
    internal EngineThread Execute(Action action)
    {
        EngineThread engineThread;
        availableThreads.WaitOne();
        lock (pool)
        {
            if (!pool.TryPop(out engineThread)) {
                engineThread = new EngineThread(this);
            }
        }
        // engineThread.Run(action)
        return engineThread;
    }
}

