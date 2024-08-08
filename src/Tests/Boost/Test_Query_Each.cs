using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;

// ReSharper disable InconsistentNaming
namespace Tests.Boost {

public static class Test_Query_Each
{
        
    [Test]
    public static void Test_Query_Each1()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1>();
        query.Each(new Each1());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(1, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Each2()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2>();
        query.Each(new Each2());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(1, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Each3()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3>();
        query.Each(new Each3());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(3, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Each4()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4>();
        query.Each(new Each4());
        foreach (var entity in store.Entities) {
            Mem.AreEqual(7, entity.GetComponent<MyComponent1>().a);
        }
    }
    
    [Test]
    public static void Test_Query_Each5()
    {
        var store       = CreateStore();
        var query       = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>();
        query.Each(new Each5());
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
    
    private readonly struct Each1 : IEach<MyComponent1>
    {
        public void Execute(ref MyComponent1 c1) => c1.a++;
    }
    
    private readonly struct Each2 : IEach<MyComponent1, MyComponent2>
    {
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2)
            => c1.a = c2.b;
    }
    
    private readonly struct Each3 : IEach<MyComponent1, MyComponent2, MyComponent3>
    {
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, ref MyComponent3 c3)
            => c1.a = c2.b + c3.b;
    }
    
    private readonly struct Each4 : IEach<MyComponent1, MyComponent2, MyComponent3, MyComponent4>
    {
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, ref MyComponent3 c3, ref MyComponent4 c4)
            => c1.a = c2.b + c3.b + c4.b;
    }
    
    private readonly struct Each5 : IEach<MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>
    {
        public void Execute(ref MyComponent1 c1, ref MyComponent2 c2, ref MyComponent3 c3, ref MyComponent4 c4, ref MyComponent5 c5)
            => c1.a = c2.b + c3.b + c4.b + c5.b; 
    }
}
}