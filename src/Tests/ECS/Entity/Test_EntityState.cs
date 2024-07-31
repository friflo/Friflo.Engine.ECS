using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;


// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_EntityState
{
    [Test]
    public static void Test_EntityState_getter()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position(1,2,3), new EntityName("test"), Tags.Get<TestTag, TestTag2>());
        
        var state = entity.State;
        IsFalse(state.IsNull);
        
        var tags = state.Tags;
        IsTrue(tags.Has<TestTag>());
        IsTrue(tags.Has<TestTag2>());
        
        AreEqual(new Position(1,2,3),   state.Get<Position>());
        AreEqual("test",                state.Get<EntityName>().value);
        
        entity.DeleteEntity();
        
        state = entity.State;
        IsTrue(state.IsNull);
        Throws<NullReferenceException>(() => {
            _ = state.Tags;
        });
        Throws<NullReferenceException>(() => {
            _ = state.Get<Position>();
        });
    }
}

}
