using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UnusedParameter.Local
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
    [Test]
    public static void Test_Index_Relations_add_remove()
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
    
    [Test]
    public static void Test_Index_Relations_query()
    {
        var store   = new EntityStore();
        var player1 = store.CreateEntity();
        var player2 = store.CreateEntity();
        var player3 = store.CreateEntity();
        var player4 = store.CreateEntity();
        player1.AddComponent(new Position());
        player1.AddComponent(new AttackRelation { target = player2, speed = 10 });
        player1.AddComponent(new AttackRelation { target = player3, speed = 11 });
        player1.AddComponent(new AttackRelation { target = player4, speed = 12 });
        {
            var query = store.Query<AttackRelation>();
            int count = 0;
            query.ForEachEntity((ref AttackRelation relation, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(10, relation.speed); break;
                    case 1: AreEqual(11, relation.speed); break;
                    case 2: AreEqual(12, relation.speed); break;
                }
            });
            AreEqual(3, count);
            AreEqual(3, query.Count);
        }
        var e = Throws<InvalidOperationException>(() => {
            store.Query<AttackRelation, Position>();
        });
        AreEqual("relation component query cannot have other query components", e!.Message);
    }
}

}
