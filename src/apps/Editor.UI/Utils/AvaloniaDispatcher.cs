// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Friflo.Engine.ECS;

namespace Friflo.Editor.Utils;

public class AvaloniaDispatcher : IStoreDispatcher
{
    public void          AssertMainThread    ()                             => Dispatcher.UIThread.VerifyAccess();
    public void          Post                (Action              action)   => Dispatcher.UIThread.Post       (action);
    public      TResult  Invoke     <TResult>(Func     <TResult>  action)   => Dispatcher.UIThread.Invoke     (action);
    public Task          InvokeAsync         (Func<Task>          action)   => Dispatcher.UIThread.InvokeAsync(action);
    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)   => Dispatcher.UIThread.InvokeAsync(action);
}