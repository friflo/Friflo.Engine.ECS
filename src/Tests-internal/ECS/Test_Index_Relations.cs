using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

internal struct AttackRelation : IRelationComponent<Entity>
{
    public int    speed;
    public Entity target;
    public Entity GetRelation() => target;

    public override string ToString() => target.ToString();
}


public static class Test_Index_Relations
{
    // [Test]
    public static void Test_Index_Relations_basics()
    {
        var store   = new EntityStore();
        var player1 = store.CreateEntity();
        var player2 = store.CreateEntity();
        var player3 = store.CreateEntity();
        IsTrue (player1.AddComponent(new AttackRelation { target = player2, speed = 1 }));
        IsTrue (player1.AddComponent(new AttackRelation { target = player3, speed = 1  }));
        IsFalse(player1.AddComponent(new AttackRelation { target = player3, speed = 42  }));
        
        IsTrue (player1.RemoveRelation<AttackRelation, Entity>(player2));
        IsFalse(player1.RemoveRelation<AttackRelation, Entity>(player2));
        
        IsTrue (player1.RemoveRelation<AttackRelation, Entity>(player3));
        IsFalse(player1.RemoveRelation<AttackRelation, Entity>(player3));
        
        var start = Mem.GetAllocatedBytes();
        player1.AddComponent(new AttackRelation { target = player2, speed = 1 });
        player1.AddComponent(new AttackRelation { target = player3, speed = 1 });
        player1.RemoveRelation<AttackRelation, Entity>(player3);
        player1.RemoveRelation<AttackRelation, Entity>(player2);
        Mem.AssertNoAlloc(start);
    }
}

}
