using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;



class TestClass
{
    public void MyMethod() {}
   
    public void ForEach() {
        var store = new EntityStore();
        store.Query(this, nameof(MyMethod), 1);
    }
}

public static class QueryExt
{
    public static void Query<TInstance, TUniform>(this EntityStore store, TInstance instance, string method, TUniform uniform) { }
    public static void Query<TInstance, TUniform>(this EntityStore store, TInstance instance, string method) { }
    
    public static void Query<TUniform>(this EntityStore store, string method, TUniform uniform) { }
    public static void Query<TUniform>(this EntityStore store, string method) { }
}

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {

public static class Test_SrcGen
{
    [Test]
    public static void Test_SrcGen_Call() {
        var store = new EntityStore();
        store.Query(nameof(TestClass.MyMethod), 1);
    }
}

}

