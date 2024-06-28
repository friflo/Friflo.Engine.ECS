using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable AccessToModifiedClosure
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
        var store       = new EntityStore();
        var entity3     = store.CreateEntity(3);
            
        var target10    = store.CreateEntity(10);
        var target11    = store.CreateEntity(11);
        IsTrue (entity3.AddComponent(new AttackRelation { target = target10, speed = 1 }));
        IsTrue (entity3.AddComponent(new AttackRelation { target = target11, speed = 1  }));
        IsFalse(entity3.AddComponent(new AttackRelation { target = target11, speed = 42  }));
        
        IsTrue (entity3.RemoveRelation<AttackRelation, Entity>(target10));
        IsFalse(entity3.RemoveRelation<AttackRelation, Entity>(target10));
        
        IsTrue (entity3.RemoveRelation<AttackRelation, Entity>(target11));
        IsFalse(entity3.RemoveRelation<AttackRelation, Entity>(target11));
        
        var start = Mem.GetAllocatedBytes();
        entity3.AddComponent(new AttackRelation { target = target10, speed = 1 });
        entity3.AddComponent(new AttackRelation { target = target11, speed = 1 });
        entity3.RemoveRelation<AttackRelation, Entity>(target11);
        entity3.RemoveRelation<AttackRelation, Entity>(target10);
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Index_Relations_query()
    {
        var store    = new EntityStore();
        var entity0  = store.CreateEntity(100);
        var emptyRelations = entity0.GetRelations<AttackRelation, Entity>();
        AreEqual(0, emptyRelations.Length);
        
        var entity1  = store.CreateEntity(1);
        var entity2  = store.CreateEntity(2);
        var entity3  = store.CreateEntity(3);
        
        var target10 = store.CreateEntity();
        var target11 = store.CreateEntity();
        var target12 = store.CreateEntity();
        
        entity1.AddComponent(new AttackRelation { target = target10, speed = 42 });
        
        entity2.AddComponent(new AttackRelation { target = target10, speed = 20 });
        entity2.AddComponent(new AttackRelation { target = target11, speed = 21 });
        
        entity3.AddComponent(new Position());
        entity3.AddComponent(new AttackRelation { target = target10, speed = 10 });
        entity3.AddComponent(new AttackRelation { target = target11, speed = 11 });
        entity3.AddComponent(new AttackRelation { target = target12, speed = 12 });
        {
            var query = store.Query<AttackRelation>();
            int count = 0;
            query.ForEachEntity((ref AttackRelation relation, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(42, relation.speed); break;
                    case 1: AreEqual(20, relation.speed); break;
                    case 2: AreEqual(21, relation.speed); break;
                    case 3: AreEqual(10, relation.speed); break;
                    case 4: AreEqual(11, relation.speed); break;
                    case 5: AreEqual(12, relation.speed); break;
                }
            });
            AreEqual(6, count);
            AreEqual(6, query.Count);
            count = 0;
            foreach (var entity in query.Entities) {
                count++;
                var relationCount = 0;
                var relations = entity.GetRelations<AttackRelation, Entity>();
                switch (entity.Id) {
                    case 1:
                        AreEqual(1,  relations.Length);
                        AreEqual(42, relations[0].speed);
                        foreach (var relation in relations) {
                            switch (relationCount++) {
                                case 0: AreEqual(42, relation.speed); break;
                            }
                        }
                        AreEqual(1, relationCount);
                        break;
                    case 2:
                        AreEqual(2,  relations.Length);
                        AreEqual(20, relations[0].speed);
                        AreEqual(21, relations[1].speed);
                        foreach (var relation in relations) {
                            switch (relationCount++) {
                                case 0: AreEqual(20, relation.speed); break;
                                case 1: AreEqual(21, relation.speed); break;
                            }
                        }
                        AreEqual(2, relationCount);
                        break;
                    case 3:
                        AreEqual(3,  relations.Length);
                        AreEqual(10, relations[0].speed);
                        AreEqual(11, relations[1].speed);
                        AreEqual(12, relations[2].speed);
                        foreach (var relation in relations) {
                            switch (relationCount++) {
                                case 0: AreEqual(10, relation.speed); break;
                                case 1: AreEqual(11, relation.speed); break;
                                case 2: AreEqual(12, relation.speed); break;
                            }
                        }
                        AreEqual(3, relationCount);
                        break;
                }
            }
            AreEqual(6, count);
        }
    }
    
    [Test]
    public static void Test_Index_Relations_Enumerator()
    {
        var store    = new EntityStore();
        var entity  = store.CreateEntity(2);

        var target10 = store.CreateEntity();
        var target11 = store.CreateEntity();
        entity.AddComponent(new AttackRelation { target = target10, speed = 20 });
        entity.AddComponent(new AttackRelation { target = target11, speed = 21 });
        
        var relations = entity.GetRelations<AttackRelation, Entity>();
        
        // --- IEnumerable<>
        IEnumerable<AttackRelation> enumerable = relations;
        var enumerator = enumerable.GetEnumerator();
        using var enumerator1 = enumerator as IDisposable;
        int count = 0;
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(2, count);
        
        count = 0;
        enumerator.Reset();
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(2, count);
        
        // --- IEnumerable
        IEnumerable enumerable2 = relations;
        count = 0;
        foreach (var relation in enumerable2) {
            count++;
        }
        AreEqual(2, count);
    }
    
    [Test]
    public static void Test_Index_Relations_query_exception()
    {
        var store    = new EntityStore();
        var e = Throws<InvalidOperationException>(() => {
            store.Query<AttackRelation, Position>();
        });
        AreEqual("relation component query cannot have other query components", e!.Message);
    }
}

}
