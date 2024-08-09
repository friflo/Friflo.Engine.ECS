using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;

// ReSharper disable InconsistentNaming
namespace Tests.Boost {

public static class Test_Query_EachEntity
{
        
    [Test]
    public static void Test_Query_EachEntity1()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1>();
        query.EachEntity(new Each1());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(1, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Chunks_EachEntity1()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1>();
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(new Each1());
        }
        foreach (var entity in store.Entities) {
            Mem.AreEqual(1, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_EachEntity2()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2>();
        query.EachEntity(new Each2());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(1, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Chunks_EachEntity2()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2>();
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(new Each2());
        }
        foreach (var entity in store.Entities) {
            Mem.AreEqual(1, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_EachEntity3()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3>();
        query.EachEntity(new Each3());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(3, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Chunks_EachEntity3()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3>();
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(new Each3());
        }
        foreach (var entity in store.Entities) {
            Mem.AreEqual(3, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_EachEntity4()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4>();
        query.EachEntity(new Each4());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(7, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Chunks_EachEntity4()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4>();
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(new Each4());
        }
        foreach (var entity in store.Entities) {
            Mem.AreEqual(7, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_EachEntity5()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>();
        query.EachEntity(new Each5());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(15, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Chunks_EachEntity5()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>();
        foreach (var chunk in query.Chunks) {
            chunk.EachEntity(new Each5());
        }
        foreach (var entity in store.Entities) {
            Mem.AreEqual(15, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    
    private static EntityStore CreateStore()
    {
        var store = new EntityStore();
        for (int n = 0; n < 100; n++) {
            store.CreateEntity(
                new MyComponent1(),
                new MyComponent2{ b = 1 },
                new MyComponent3{ b = 2 },
                new MyComponent4{ b = 4 },
                new MyComponent5{ b = 8 });
        }
        return store;
    }
    
    private struct Each1 : IEachEntity<MyComponent1>
    {
        int count;
        public void Execute(ref MyComponent1 c1, int id) {
            c1.a++;
            Mem.AreEqual(++count, id);
        }
    }
    
    private struct Each2 : IEachEntity<MyComponent1, MyComponent2>
    {
        int count;
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, int id) {
            c1.a = c2.b;
            Mem.AreEqual(++count, id);
        }
    }
    
    private struct Each3 : IEachEntity<MyComponent1, MyComponent2, MyComponent3>
    {
        int count;
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, ref MyComponent3 c3, int id) {
            c1.a = c2.b + c3.b;
            Mem.AreEqual(++count, id);
        }
    }
    
    private struct Each4 : IEachEntity<MyComponent1, MyComponent2, MyComponent3, MyComponent4>
    {
        int count;
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, ref MyComponent3 c3, ref MyComponent4 c4, int id) {
            c1.a = c2.b + c3.b + c4.b;
            Mem.AreEqual(++count, id);
        }
    }
    
    private struct Each5 : IEachEntity<MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>
    {
        int count;
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, ref MyComponent3 c3, ref MyComponent4 c4, ref MyComponent5 c5, int id) {
            c1.a = c2.b + c3.b + c4.b + c5.b;
            Mem.AreEqual(++count, id);
        }
    }
}
}