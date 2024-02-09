using System.Threading;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable once InconsistentNaming
namespace Tests.ECS.Arch;

public static class Test_QueryJobArg
{
    private static void SetComponent(Chunk<MyComponent1> myComponent1, ChunkEntities _, ref int count)
    {
        var span = myComponent1.Span;
        Interlocked.Add(ref count, span.Length);
        for (int n = 0; n < span.Length; n++) {
            ++span[n].a;
        }
    }
    
    [Test]
    public static void Test_QueryJobArg_RunParallel_Arg_1()
    {
        var entityCount     = 500;
        using var runner    = new ParallelJobRunner(4, "TestRunner");
        var store           = new EntityStore(PidType.UsePidAsId) { JobRunner = runner };
        var archetype       = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        var entities        = new Entity[entityCount];
        var query           = store.Query<MyComponent1>();
        
        var count           = 0;
        var forEach = query.ForEach((chunk, chunkEntities) => {
            Mem.IsTrue(chunkEntities.Execution == JobExecution.Parallel);
            SetComponent(chunk, chunkEntities, ref count);
        });
        forEach.MinParallelChunkLength = 1;
        
        for (int n = 0; n < entityCount; n++)
        {
            for (int i = 0; i < n; i++) { entities[i].GetComponent<MyComponent1>().a = i; }
            
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
}