using System;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {


public static class Test_StoreDispatcher
{
    [Test]
    public static void Test_StoreDispatcher_sync()
    {
        // --- int Invoke()
        var result = StoreDispatcher.Invoke(() => 42);
        AreEqual(42, result);
        
        // --- void Post()
        var called = false;
        StoreDispatcher.Post(() => {
            called = true;
        });
        IsTrue(called);
    }
    
    [Test]
    public  static void  Test_StoreDispatcher_async() => SingleThreadSynchronizationContext.Run(async () => await StoreDispatcher_async());
    private static async Task StoreDispatcher_async()
    {
        // --- Task<int> InvokeAsync()
        var result = await StoreDispatcher.InvokeAsync(() => Task.FromResult(42));
        AreEqual(42, result);
        
        // --- Task InvokeAsync()
        var called = false;
        await StoreDispatcher.InvokeAsync(() => {
            called = true;
            return Task.CompletedTask;
        });
        IsTrue(called);
    }
    
    [Test]
    public static void Test_StoreDispatcher_AssertMainThread()
    {
        StoreDispatcher.AssertMainThread();
    }

    [Test]
    public static void Test_StoreDispatcher_SetDispatcher()
    {
        var e = Throws<ArgumentNullException>(() => {
            StoreDispatcher.SetDispatcher(null);
        });
        AreEqual("dispatcher", e!.ParamName);
        
        var dispatcher = new TestDispatcher();
        IStoreDispatcher old = null;
        try {
            old = StoreDispatcher.SetDispatcher(dispatcher);
        
        }
        finally {
            StoreDispatcher.SetDispatcher(old);
        }
    }
}

internal class TestDispatcher : IStoreDispatcher
{
    public void          AssertMainThread    ()                             { }
    public void          Post                (Action              action)   => action();
    public      TResult  Invoke     <TResult>(Func     <TResult>  action)   => action();
    public Task          InvokeAsync         (Func<Task>          action)   => action();
    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)   => action();
}

}
