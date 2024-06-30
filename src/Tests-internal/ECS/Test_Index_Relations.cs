using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Relations;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UnusedVariable
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

internal struct IntRelation : IRelationComponent<int>
{
    public int value;
    public int GetRelation() => value;

    public override string ToString() => value.ToString();
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
        var components  = entity3.Components;
        IsTrue (entity3.AddComponent(new AttackRelation { target = target10, speed = 1 }));
        AreEqual("Components: [] +1 relations", components.ToString());
        IsTrue (entity3.AddComponent(new AttackRelation { target = target11, speed = 1  }));
        AreEqual("Components: [] +2 relations", components.ToString());
        IsFalse(entity3.AddComponent(new AttackRelation { target = target11, speed = 42  }));
        AreEqual("Components: [] +2 relations", components.ToString());
        
        IsTrue (entity3.RemoveRelation<AttackRelation, Entity>(target10));
        AreEqual("Components: [] +1 relations", components.ToString());
        IsFalse(entity3.RemoveRelation<AttackRelation, Entity>(target10));
        AreEqual("Components: [] +1 relations", components.ToString());
        
        IsTrue (entity3.RemoveRelation<AttackRelation, Entity>(target11));
        AreEqual("Components: []", components.ToString());
        IsFalse(entity3.RemoveRelation<AttackRelation, Entity>(target11));
        AreEqual("Components: []", components.ToString());
        
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
        
        emptyRelations = entity0.GetRelations<AttackRelation, Entity>();
        AreEqual(0, emptyRelations.Length);
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
    
#pragma warning disable CS0618 // Type or member is obsolete
    [Test]
    public static void Test_Index_Relations_EntityComponents()
    {
        var store    = new EntityStore();
        var target10 = store.CreateEntity(10);
        var target11 = store.CreateEntity(11);
        var entity   = store.CreateEntity(1);

        var components = entity.Components;
        entity.AddComponent(new Position());
        AreEqual(1, components.Count);

        entity.AddComponent(new AttackRelation { target = target10, speed = 20 });
        AreEqual("Components: [Position] +1 relations", components.ToString());
        AreEqual(2, components.Count);
        int count = 0;
        foreach (var component in components) {
            switch (count++) {
                case 0:
                    AreEqual(component.Type.Type, typeof(Position));
                    break;
                case 1:
                    AreEqual(component.Type.Type, typeof(AttackRelation));
                    AreEqual(20, ((AttackRelation)component.Value).speed );
                    break;
            }
        }
        
        entity.AddComponent(new AttackRelation { target = target11, speed = 21 });
        AreEqual("Components: [Position] +2 relations", components.ToString());
        AreEqual(3, components.Count);
        count = 0;
        foreach (var component in components) {
            switch (count++) {
                case 0:
                    AreEqual(component.Type.Type, typeof(Position));
                    break;
                case 1:
                    AreEqual(component.Type.Type, typeof(AttackRelation));
                    AreEqual(20, ((AttackRelation)component.Value).speed );
                    break;
                case 2:
                    AreEqual(component.Type.Type, typeof(AttackRelation));
                    AreEqual(21, ((AttackRelation)component.Value).speed );
                    break;
            }
        }
    }
    
    [Test]
    public static void Test_Index_Relations_Enumerator()
    {
        var store    = new EntityStore();
        var entity  = store.CreateEntity(1);

        var target10 = store.CreateEntity(10);
        var target11 = store.CreateEntity(11);
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
        
        var entity2  = store.CreateEntity(2);
        enumerable2  = entity2.GetRelations<AttackRelation, Entity>();
        count = 0;
        foreach (var relation in enumerable2) {
            count++;
        }
        AreEqual(0, count);
    }
    
    [Test]
    public static void Test_Index_Relations_adjust_position_on_remove_relation()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddComponent(new IntRelation { value = 1 });
        entity1.AddComponent(new IntRelation { value = 2 });
        // last relation IntRelation { value = 2 } at StructHeap<>.components[1] is moved to components[0]
        // So its stored position need to be updated 
        entity1.RemoveRelation<IntRelation, int>(1);
        entity2.AddComponent(new IntRelation { value = 3 });
        
        var relations1 = entity1.GetRelations<IntRelation, int>();
        var relations2 = entity2.GetRelations<IntRelation, int>();
        AreEqual(1, relations1.Length);
        AreEqual(2, relations1[0].value);
        AreEqual(1, relations2.Length);
        AreEqual(3, relations2[0].value);
        
        AreEqual("Relations<IntRelation>[1]", relations1.ToString());
    }
    
    [Test]
    public static void Test_Index_Relations_int_relation()
    {
        var relationCount   = 10;
        var entityCount     = 100;
        var store           = new EntityStore();
        var entities        = new List<Entity>();
        for (int n = 0; n < entityCount; n++) {
            entities.Add(store.CreateEntity());
        }
        foreach (var entity in entities) {
            for (int n = 0; n < relationCount; n++) {
                Mem.AreEqual(n, entity.GetRelations<IntRelation,int>().Length);
                Mem.IsTrue(entity.AddComponent(new IntRelation{ value = n }));
            }
        }
        foreach (var entity in entities) {
            for (int n = 0; n < relationCount; n++) {
                Mem.AreEqual(relationCount - n, entity.GetRelations<IntRelation,int>().Length);
                Mem.IsTrue(entity.RemoveRelation<IntRelation, int>(n));
            }
        }
        var start = Mem.GetAllocatedBytes();
        foreach (var entity in entities) {
            for (int n = 0; n < relationCount; n++) {
                Mem.IsTrue(entity.AddComponent(new IntRelation{ value = n }));
            }
            Mem.AreEqual(relationCount, entity.Components.Count);
        }
        foreach (var entity in entities) {
            for (int n = 0; n < relationCount; n++) {
                Mem.IsTrue(entity.RemoveRelation<IntRelation, int>(n));
            }
            Mem.AreEqual(0, entity.Components.Count);
        }
        Mem.AssertNoAlloc(start);
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
