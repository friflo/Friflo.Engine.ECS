// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Contains methods to dispatch execution of <see cref="Action"/>'s or <see cref="Func{TResult}"/>'s to the main thread.
/// </summary>
/// <remarks>
/// These methods are required to access an <see cref="ECS.EntityStore"/> as instances of this class are not thread safe. 
/// Note: This file may be moved to project: <see cref="Friflo.Engine.ECS"/>
/// Method mapping for various UI application libraries.
/// <list type="bullet">
///   <item> <b>AvaloniaUI</b> - methods map to <c>Avalonia.Threading.Dispatcher.UIThread</c> methods. <br/> </item>
///   <item> <b>MAUI</b> - methods map to <c>Microsoft.Maui.ApplicationModel.MainThread</c> methods. <br/> </item>
///   <item> <b>WinForms</b> - methods map to <c>System.Windows.Threading.Dispatcher</c> methods. <br/> </item>
/// </list>
/// </remarks>
public static class StoreDispatcher
{
    private static readonly DefaultDispatcher   DefaultDispatcher   = new DefaultDispatcher();
    private static          IStoreDispatcher    _dispatcher         = DefaultDispatcher;
    
    
    public static IStoreDispatcher SetDispatcher(IStoreDispatcher dispatcher) {
        if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
        dispatcher.Post(() => {
            int threadId = Environment.CurrentManagedThreadId;
            Console.WriteLine($"{nameof(StoreDispatcher)} - Set dispatcher to thread id: {threadId}");                
        });
        var old = _dispatcher;
        _dispatcher = dispatcher;
        return old;
    }
    
    public static   void            AssertMainThread()                               => _dispatcher.AssertMainThread();
    public static   void            Post                (Action              action) => _dispatcher.Post        (action);
    public static        TResult    Invoke<TResult>     (Func     <TResult>  action) => _dispatcher.Invoke      (action);
    public static   Task            InvokeAsync         (Func<Task>          action) => _dispatcher.InvokeAsync (action);
    public static   Task<TResult>   InvokeAsync<TResult>(Func<Task<TResult>> action) => _dispatcher.InvokeAsync (action);
}

public interface IStoreDispatcher
{
    public  void            AssertMainThread();
    public  void            Post                (Action                 action);
    public       TResult    Invoke     <TResult>(Func     <TResult>     action);
    public  Task            InvokeAsync         (Func<Task>             action);
    public  Task<TResult>   InvokeAsync<TResult>(Func<Task<TResult>>    action);
}

internal sealed class DefaultDispatcher : IStoreDispatcher
{
    public void          AssertMainThread    ()                             { }
    public void          Post                (Action              action)   => action();
    public      TResult  Invoke     <TResult>(Func     <TResult>  action)   => action();
    public Task          InvokeAsync         (Func<Task>          action)   => action();
    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)   => action();
}
