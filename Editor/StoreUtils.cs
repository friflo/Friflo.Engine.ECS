// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Friflo.Fliox.Editor;

public static class StoreUtils
{
    public static void AssertUIThread()
    {
        Dispatcher.UIThread.VerifyAccess();
    }

    public static void Post(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
    
    public static TResult Invoke<TResult>(Func<TResult> action)
    {
        return Dispatcher.UIThread.Invoke(action);
    }
    
    public static async Task InvokeAsync(Func<Task> action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }
    
    public static async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)
    {
        return await Dispatcher.UIThread.InvokeAsync(action);
    }
}