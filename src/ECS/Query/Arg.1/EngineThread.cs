// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable CheckNamespace
namespace Friflo.Engine.ECS;

internal sealed class EngineThread
{
    private  readonly   Thread          thread;
    internal readonly   AutoResetEvent  finished;
    
    internal EngineThread() {
        thread = new Thread(Run);
    }
    
    internal void Run()
    {

    }
}


internal sealed class EngineThreadPool
{
    internal static readonly EngineThreadPool Instance = new();
        
    private readonly Stack<EngineThread> pool = new ();
    
    
    internal EngineThread Execute(Action action)
    {
        EngineThread engineThread;
        lock (pool)
        {
            if (!pool.TryPop(out engineThread)) {
                engineThread = new EngineThread();
            }
        }
        // engineThread.Run(action);
        return engineThread;
    }
}

