// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.Utils;

public class AvaloniaDispatcher : IMainThreadDispatcher
{
    public void AssertMainThread()
    {
        Dispatcher.UIThread.VerifyAccess();
    }

    public void Post(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
    
    public TResult Invoke<TResult>(Func<TResult> action)
    {
        return Dispatcher.UIThread.Invoke(action);
    }
    
    public async Task InvokeAsync(Func<Task> action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }
    
    public async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)
    {
        return await Dispatcher.UIThread.InvokeAsync(action);
    }
}