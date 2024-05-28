using System.Threading;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable once InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_QueryJobArg
{
#region general
    private static ParallelJobRunner _jobRunner;
#if UNITY_5_3_OR_NEWER
    private const int EntityCount = 102;
#else
    private const int EntityCount = 502;
#endif

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
    
    private static void AssertExecution(in ChunkEntities entities)
    {
        if (entities.Length >= EntityCount - 2) {
            Mem.IsTrue(entities.Execution == JobExecution.Sequential);
        } else {
            Mem.IsTrue(entities.Execution == JobExecution.Parallel);
        }
    }
    
    private static void Run(QueryJob queryJob, int iteration)
    {
        if (iteration == EntityCount - 1) {
            queryJob.Run();
            return;
        }
        if (iteration == EntityCount - 2) {
            queryJob.MinParallelChunkLength = 1000; // execute last iteration sequential
        }
        queryJob.RunParallel();
    }
    
    #endregion
    
    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_1()
    {

        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        var entities    = new Entity[EntityCount];

        var query       = store.Query<MyComponent1>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, chunkEntities) => {
            AssertExecution(chunkEntities);
            var span1 = myComponent1.Span;
            var ids = chunkEntities.Ids;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                Mem.AreEqual(span1[n].a + 1, ids[n]);
                ++span1[n].a;
            }
        });
        Assert.AreEqual(16, forEach.ParallelComponentMultiple);
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < EntityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                entities[i].GetComponent<MyComponent1>().a = i;
            }
            // method under test
            count = 0;
            Run(forEach, n);
            
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
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2>());
        var entities    = new Entity[EntityCount];

        var query       = store.Query<MyComponent1, MyComponent2>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, myComponent2, chunkEntities) => {
            AssertExecution(chunkEntities);
            var span1 = myComponent1.Span;
            var span2 = myComponent2.Span;
            var ids = chunkEntities.Ids;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                Mem.AreEqual(span1[n].a + 1, ids[n]);
                ++span1[n].a;
                ++span2[n].b;
            }
        });
        Assert.AreEqual(16, forEach.ParallelComponentMultiple);
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < EntityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                entity.GetComponent<MyComponent1>().a = i;
                entity.GetComponent<MyComponent2>().b = i;
            }
            // method under test
            count = 0;
            Run(forEach, n);
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                var comp1 = entity.GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp1.a);
                var comp2 = entity.GetComponent<MyComponent2>();
                Mem.AreEqual(i + 1, comp2.b);
            }
            entities[n] = archetype.CreateEntity();
        }
    }
    
    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_3()
    {
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, Position>());
        var entities    = new Entity[EntityCount];

        var query       = store.Query<MyComponent1, MyComponent2, Position>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, myComponent2, position, chunkEntities) => {
            AssertExecution(chunkEntities);
            var span1 = myComponent1.Span;
            var span2 = myComponent2.Span;
            var span3 = position.Span;
            var ids = chunkEntities.Ids;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                Mem.AreEqual(span1[n].a + 1, ids[n]);
                ++span1[n].a;
                ++span2[n].b;
                ++span3[n].x;
            }
        });
        Assert.AreEqual(16, forEach.ParallelComponentMultiple);
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < EntityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                entity.GetComponent<MyComponent1>().a = i;
                entity.GetComponent<MyComponent2>().b = i;
                entity.GetComponent<Position>().x     = i;
            }
            // method under test
            count = 0;
            Run(forEach, n);
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                var comp1 = entity.GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp1.a);
                var comp2 = entity.GetComponent<MyComponent2>();
                Mem.AreEqual(i + 1, comp2.b);
                var comp3 = entity.GetComponent<Position>();
                Mem.AreEqual(i + 1, comp3.x);
            }
            entities[n] = archetype.CreateEntity();
        }
    }
    
    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_4()
    {
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, Position, Rotation>());
        var entities    = new Entity[EntityCount];

        var query       = store.Query<MyComponent1, MyComponent2, Position, Rotation>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, myComponent2, position, rotation, chunkEntities) => {
            AssertExecution(chunkEntities);
            var span1 = myComponent1.Span;
            var span2 = myComponent2.Span;
            var span3 = position.Span;
            var span4 = rotation.Span;
            var ids = chunkEntities.Ids;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                Mem.AreEqual(span1[n].a + 1, ids[n]);
                ++span1[n].a;
                ++span2[n].b;
                ++span3[n].x;
                ++span4[n].x;
            }
        });
        Assert.AreEqual(16, forEach.ParallelComponentMultiple);
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < EntityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                entity.GetComponent<MyComponent1>().a = i;
                entity.GetComponent<MyComponent2>().b = i;
                entity.GetComponent<Position>().x     = i;
                entity.GetComponent<Rotation>().x     = i;
            }
            // method under test
            count = 0;
            Run(forEach, n);
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                var comp1 = entity.GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp1.a);
                var comp2 = entity.GetComponent<MyComponent2>();
                Mem.AreEqual(i + 1, comp2.b);
                var comp3 = entity.GetComponent<Position>();
                Mem.AreEqual(i + 1, comp3.x);
                var comp4 = entity.GetComponent<Rotation>();
                Mem.AreEqual(i + 1, comp4.x);
            }
            entities[n] = archetype.CreateEntity();
        }
    }
    
    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_5()
    {
        var store       = CreateTestStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<MyComponent1, MyComponent2, Position, Rotation, Scale3>());
        var entities    = new Entity[EntityCount];

        var query       = store.Query<MyComponent1, MyComponent2, Position, Rotation, Scale3>();
        var count       = 0;
        var forEach = query.ForEach((myComponent1, myComponent2, position, rotation, scale3, chunkEntities) => {
            AssertExecution(chunkEntities);
            var span1 = myComponent1.Span;
            var span2 = myComponent2.Span;
            var span3 = position.Span;
            var span4 = rotation.Span;
            var span5 = scale3.Span;
            var ids = chunkEntities.Ids;
            Interlocked.Add(ref count, span1.Length);
            for (int n = 0; n < span1.Length; n++) {
                Mem.AreEqual(span1[n].a + 1, ids[n]);
                ++span1[n].a;
                ++span2[n].b;
                ++span3[n].x;
                ++span4[n].x;
                ++span5[n].x;
            }
        });
        Assert.AreEqual(16, forEach.ParallelComponentMultiple);
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < EntityCount; n++)
        {
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                entity.GetComponent<MyComponent1>().a = i;
                entity.GetComponent<MyComponent2>().b = i;
                entity.GetComponent<Position>().x     = i;
                entity.GetComponent<Rotation>().x     = i;
                entity.GetComponent<Scale3>().x         = i;
            }
            // method under test
            count = 0;
            Run(forEach, n);
            
            Mem.AreEqual(n, count);
            for (int i = 0; i < n; i++) {
                var entity = entities[i];
                var comp1 = entity.GetComponent<MyComponent1>();
                Mem.AreEqual(i + 1, comp1.a);
                var comp2 = entity.GetComponent<MyComponent2>();
                Mem.AreEqual(i + 1, comp2.b);
                var comp3 = entity.GetComponent<Position>();
                Mem.AreEqual(i + 1, comp3.x);
                var comp4 = entity.GetComponent<Rotation>();
                Mem.AreEqual(i + 1, comp4.x);
                var comp5 = entity.GetComponent<Scale3>();
                Mem.AreEqual(i + 1, comp5.x);
            }
            entities[n] = archetype.CreateEntity();
        }
    }

}

}