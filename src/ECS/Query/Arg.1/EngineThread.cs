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
    
    internal EngineThread(EngineThreadPool pool, int id) {
        this.pool   = pool;
        thread      = new Thread(Run);
        thread.Name = $"EngineThread {id}";
    }
    
    internal void Run()
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
    
    internal readonly   Stack<EngineThread>     stack;
    internal readonly   Semaphore               availableThreads;
    private             int                     threadSeq;
    
    private EngineThreadPool()
    {
        stack               = new Stack<EngineThread>();
        availableThreads    = new Semaphore(0, Environment.ProcessorCount, "available engine threads");
    }
    
    internal EngineThread Execute(Action action)
    {
        EngineThread engineThread;
        availableThreads.WaitOne();
        lock (stack)
        {
            if (!stack.TryPop(out engineThread)) {
                engineThread = new EngineThread(this, ++threadSeq);
            }
        }
        // engineThread.Run(action)
        return engineThread;
    }
}

