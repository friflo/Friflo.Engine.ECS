using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.CommandBuffer;

#pragma warning disable CS0618 // Type or member is obsolete

public static class Test_CommandBuffer
{
    [Test]
    public static void Test_CommandBuffer_queue_commands()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        var ecb     = new EntityCommandBuffer(store);
        
        ecb.AddComponent(entity, new Position());
        ecb.SetComponent(entity, new Position());
        // ecb.RemoveComponent<Position>(entity);
        
        ecb.Playback();
        
        AreSame(entity.Archetype, store.GetArchetype(ComponentTypes.Get<Position>()));
    }
}