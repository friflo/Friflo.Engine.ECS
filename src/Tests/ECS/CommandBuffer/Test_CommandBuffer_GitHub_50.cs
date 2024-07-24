using System;
using System.Threading;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;

// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Buffer {

// https://github.com/friflo/Friflo.Json.Fliox/discussions/50
public static class Test_CommandBuffer_GitHub_50
{
    [Test]
    public static void Test_CommandBuffer_Parallel()
    {
        int count = 1000; // must be > ParallelComponentMultiple (16 for MyComponent1)
        var store = new EntityStore();
        for (int n = 0; n < count; n++) {
            store.CreateEntity(new MyComponent1() );
        }
        var root = new SystemRoot(store) {
            new ParallelPositionSystem()
        };
        for (int n = 0; n < count; n++) {
            root.Update(new UpdateTick(1, n)); 
        }
        Assert.AreEqual(0, store.Count);
    }
    
    class ParallelPositionSystem : QuerySystem<MyComponent1>
    {
        readonly ParallelJobRunner runner = new (Environment.ProcessorCount);
        
        protected override void OnUpdate()
        {
            var commandBuffer   = Query.Store.GetCommandBuffer();
            var synced          = commandBuffer.Synced;
            int deleteCount     = 0;
            var elementJob = Query.ForEach((_, entities) =>
            {
                if (entities.Length > 0) {
                    synced.DeleteEntity(entities[0]);
                    Interlocked.Increment(ref deleteCount);
                }
            });
            Assert.AreEqual(16, elementJob.ParallelComponentMultiple);
            elementJob.MinParallelChunkLength = 1;
            elementJob.JobRunner = runner;
            elementJob.RunParallel();

            Assert.AreEqual(deleteCount, commandBuffer.EntityCommandsCount);
            synced.Playback();
        }
    }
}
}