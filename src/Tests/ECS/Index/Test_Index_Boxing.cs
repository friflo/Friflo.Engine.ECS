using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {


[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
[SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations")]
public static class Test_Index_Boxing
{
    [Test]
    public static void Test_Index_Boxing_AvoidBoxing()
    {
        var indexedName = new IndexedInt { value = 123 };
        
        var getter = CreateGetValue<IndexedInt,int>();
        var value = getter(indexedName);
        AreEqual(123, value);
        
        var start = GC.GetAllocatedBytesForCurrentThread();
        getter(indexedName);
        var diff = GC.GetAllocatedBytesForCurrentThread() - start;
        Console.WriteLine($"diff: {diff}");
    }
    
    // https://stackoverflow.com/questions/61185678/c-sharp-casting-t-where-t-struct-to-an-interface-without-boxing
    private static GetIndexedValue<T,V> CreateGetValue<T,V>() where T : struct, IComponent
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(Test_Index_Boxing).GetMethod(nameof(GetIndexedComponentValue), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(T), typeof(V));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetIndexedValue<T,V>), genericMethod);
        return (GetIndexedValue<T,V>)genericDelegate;
    }
    
    internal static V GetIndexedComponentValue<T,V>(in T component) where T : struct, IIndexedComponent<V> {
        return component.GetIndexedValue();
    }
    
    private delegate V GetIndexedValue<T, out V>(in T component) where T : struct, IComponent;
}

}
