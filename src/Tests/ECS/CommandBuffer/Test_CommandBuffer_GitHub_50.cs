using System;
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
    [Ignore("FixMe: Add locking CommandBuffer")][Test]
    public static void Test_CommandBuffer_Parallel()
    {
        int count = 20; // must be > ParallelComponentMultiple (16 for MyComponent1)
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
            CommandBuffer cmdBuffer = Query.Store.GetCommandBuffer();
            var elementJob = Query.ForEach((transients, entities) =>
            {
                if (entities.Length > 0) {
                    cmdBuffer.DeleteEntity(entities[0]);
                }
            });
            Assert.AreEqual(16, elementJob.ParallelComponentMultiple);
            elementJob.MinParallelChunkLength = 1;
            elementJob.JobRunner = runner;
            elementJob.RunParallel();

            cmdBuffer.Playback();
        }
    }
}
}