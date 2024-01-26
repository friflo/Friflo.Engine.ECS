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
        var entity  = store.CreateEntity(1);
        var ecb     = new EntityCommandBuffer(store);
        
        var pos1 = new Position(1, 1, 1);
        var pos2 = new Position(2, 2, 2);
        ecb.AddComponent(1, pos1);
        ecb.SetComponent(1, pos2);
        //
        ecb.Playback();
        
        AreEqual(pos2.x,            entity.GetComponent<Position>().x);
        AreSame(entity.Archetype,   store.GetArchetype(ComponentTypes.Get<Position>()));
        
        ecb.AddComponent(1, pos1);
        ecb.RemoveComponent<Position>(1);
        
        ecb.Playback();
        
        IsFalse(entity.HasComponent<Position>());
    }
}