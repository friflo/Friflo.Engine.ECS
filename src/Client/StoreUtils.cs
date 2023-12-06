// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Friflo.Fliox.Engine.Client;

public static class StoreUtils
{
    private static IMainThreadDispatcher _dispatcher;
    
    public static void SetDispatcher(IMainThreadDispatcher dispatcher) {
        if (_dispatcher != null) {
            throw new InvalidOperationException("dispatcher already set");
        }
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }
    
    public static   void            AssertUIThread()                                 => _dispatcher.AssertUIThread();
    public static   void            Post                (Action              action) => _dispatcher.Post(action);
    public static   TResult         Invoke<TResult>     (Func<TResult>       action) => _dispatcher.Invoke(action);
    public static   Task            InvokeAsync         (Func<Task>          action) => _dispatcher.InvokeAsync(action);
    public static   Task<TResult>   InvokeAsync<TResult>(Func<Task<TResult>> action) => _dispatcher.InvokeAsync(action);
}

public interface IMainThreadDispatcher
{
    public  void            AssertUIThread();
    public  void            Post(Action action);
    public  TResult         Invoke<TResult>(Func<TResult> action);
    public  Task            InvokeAsync(Func<Task> action);
    public  Task<TResult>   InvokeAsync<TResult>(Func<Task<TResult>> action);
}
