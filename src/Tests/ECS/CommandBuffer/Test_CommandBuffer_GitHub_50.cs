using System;
using System.Numerics;
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
        int count = 100_000;
        var store = new EntityStore();
        for (int n = 0; n < count; n++) {
            store.CreateEntity(new TransientComponent { LifeTime = n } );
        }
        var root = new SystemRoot(store) {
            new ParallelPositionSystem()
        };
        for (int n = 0; n < count; n++) {
            root.Update(new UpdateTick(1, n)); 
        }
        Assert.AreEqual(0, store.Count);
    }

    struct TransientComponent : IComponent
    {
        public float    LifeTime;
        public Vector3  Position;
        public Vector3  Velocity;
    }
    
    class ParallelPositionSystem : QuerySystem<TransientComponent>
    {
        readonly ParallelJobRunner runner = new (Environment.ProcessorCount);
        
        protected override void OnUpdate()
        {
            CommandBuffer cmdBuffer = Query.Store.GetCommandBuffer();
            var elementJob = Query.ForEach((transients, entities) =>
            {
                var counter = 0;
                foreach (ref var transient in transients.Span)
                {
                    transient.LifeTime -= Tick.deltaTime;
                    if (transient.LifeTime <= 0) {
                        cmdBuffer.DeleteEntity(entities[counter]);
                    } else {
                        transient.Position += transient.Velocity * Tick.deltaTime;
                    }
                    counter++;
                }
            });
            elementJob.JobRunner = runner;
            elementJob.RunParallel();

            cmdBuffer.Playback();
        }
    }
}
}