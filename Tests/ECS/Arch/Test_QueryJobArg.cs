using System.Threading;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable once InconsistentNaming
namespace Tests.ECS.Arch;

public static class Test_QueryJobArg
{
#region general
    private static ParallelJobRunner _jobRunner;

    [OneTimeSetUp]
    public static void Setup() {
        _jobRunner = new ParallelJobRunner(5, "TestRunner");
    }
    
    [OneTimeTearDown]
    public static void TearDown() {
        _jobRunner.Dispose();
        _jobRunner = null;
    }
    
    private static EntityStore CreateTestStore() {
        return new EntityStore(PidType.UsePidAsId) { JobRunner = _jobRunner };
    }
    #endregion
    
    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_1()
    {
        var entityCount = 500;
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        var entities    = new Entity[entityCount];

        var query       = store.Query<MyComponent1>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, chunkEntities) => {
            Mem.IsTrue(chunkEntities.Execution == JobExecution.Parallel);
            var span = myComponent1.Span;
            Interlocked.Add(ref count, span.Length);
            for (int n = 0; n < span.Length; n++) {
                ++span[n].a;
            }
        });
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < entityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                entities[i].GetComponent<MyComponent1>().a = i;
            }
            // method under test
            count = 0;
            forEach.RunParallel();
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var comp = entities[i].GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp.a);
            }
            entities[n] = archetype.CreateEntity();
        }
    }

    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_2()
    {
        var entityCount = 500;
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2>());
        var entities    = new Entity[entityCount];

        var query       = store.Query<MyComponent1, MyComponent2>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, myComponent2, chunkEntities) => {
            Mem.IsTrue(chunkEntities.Execution == JobExecution.Parallel);
            var span1 = myComponent1.Span;
            var span2 = myComponent2.Span;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                ++span1[n].a;
                ++span2[n].b;
            }
        });
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < entityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                entity.GetComponent<MyComponent1>().a = i;
                entity.GetComponent<MyComponent2>().b = i;
            }
            // method under test
            count = 0;
            forEach.RunParallel();
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var comp1 = entities[i].GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp1.a);
                var comp2 = entities[i].GetComponent<MyComponent2>();
                Mem.AreEqual(i + 1, comp2.b);
            }
            entities[n] = archetype.CreateEntity();
        }
    }
    
    // [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_3()
    {
        var entityCount = 500;
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, Position>());
        var entities    = new Entity[entityCount];

        var query       = store.Query<MyComponent1, MyComponent2, Position>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, myComponent2, position, chunkEntities) => {
            Mem.IsTrue(chunkEntities.Execution == JobExecution.Parallel);
            var span1 = myComponent1.Span;
            var span2 = myComponent2.Span;
            var span3 = position.Span;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                ++span1[n].a;
                ++span2[n].b;
                ++span3[n].x;
            }
        });
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < entityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                entity.GetComponent<MyComponent1>().a = i;
                entity.GetComponent<MyComponent2>().b = i;
                entity.GetComponent<Position>().x     = i;
            }
            // method under test
            count = 0;
            forEach.RunParallel();
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var comp1 = entities[i].GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp1.a);
                var comp2 = entities[i].GetComponent<MyComponent2>();
                Mem.AreEqual(i + 1, comp2.b);
                var comp3 = entities[i].GetComponent<Position>();
                Mem.AreEqual(i + 1, comp3.x);
            }
            entities[n] = archetype.CreateEntity();
        }
    }

}