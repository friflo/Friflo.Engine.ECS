using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations {

public static class Test_Relations_Query
{
    [Test]
    public static void Test_Relations_query()
    {
        var store    = new EntityStore();
        var entity0  = store.CreateEntity(100);
        var emptyRelations = entity0.GetRelations<AttackRelation>();
        AreEqual(0, emptyRelations.Length);
        
        var entity1  = store.CreateEntity(1);
        var entity2  = store.CreateEntity(2);
        var entity3  = store.CreateEntity(3);
        
        var target10 = store.CreateEntity();
        var target11 = store.CreateEntity();
        var target12 = store.CreateEntity();
        
        entity1.AddRelation(new AttackRelation { target = target10, speed = 42 });
        
        entity2.AddRelation(new AttackRelation { target = target10, speed = 20 });
        entity2.AddRelation(new AttackRelation { target = target11, speed = 21 });
        
        entity3.AddComponent(new Position());
        entity3.AddRelation(new AttackRelation { target = target10, speed = 10 });
        entity3.AddRelation(new AttackRelation { target = target11, speed = 11 });
        entity3.AddRelation(new AttackRelation { target = target12, speed = 12 });
        
        emptyRelations = entity0.GetRelations<AttackRelation>();
        AreEqual(0, emptyRelations.Length);
        
        // --- query
        var query = store.Query<AttackRelation>();
        int count = 0;
        query.ForEachEntity((ref AttackRelation relation, Entity entity) => {
            switch (count++) {
                case 0: Mem.AreEqual(42, relation.speed); break;
                case 1: Mem.AreEqual(20, relation.speed); break;
                case 2: Mem.AreEqual(21, relation.speed); break;
                case 3: Mem.AreEqual(10, relation.speed); break;
                case 4: Mem.AreEqual(11, relation.speed); break;
                case 5: Mem.AreEqual(12, relation.speed); break;
            }
        });
        Mem.AreEqual(6, count);
        Mem.AreEqual(6, query.Count);
        
        var start = Mem.GetAllocatedBytes();
        count = 0;
        foreach (var entity in query.Entities) {
            count++;
            var relationCount = 0;
            var relations = entity.GetRelations<AttackRelation>();
            switch (entity.Id) {
                case 1:
                    Mem.AreEqual(1,  relations.Length);
                    Mem.AreEqual(42, relations[0].speed);
                    foreach (var relation in relations) {
                        switch (relationCount++) {
                            case 0: Mem.AreEqual(42, relation.speed); break;
                        }
                    }
                    Mem.AreEqual(1, relationCount);
                    break;
                case 2:
                    Mem.AreEqual(2,  relations.Length);
                    Mem.AreEqual(20, relations[0].speed);
                    Mem.AreEqual(21, relations[1].speed);
                    foreach (var relation in relations) {
                        switch (relationCount++) {
                            case 0: Mem.AreEqual(20, relation.speed); break;
                            case 1: Mem.AreEqual(21, relation.speed); break;
                        }
                    }
                    Mem.AreEqual(2, relationCount);
                    break;
                case 3:
                    Mem.AreEqual(3,  relations.Length);
                    Mem.AreEqual(10, relations[0].speed);
                    Mem.AreEqual(11, relations[1].speed);
                    Mem.AreEqual(12, relations[2].speed);
                    foreach (var relation in relations) {
                        switch (relationCount++) {
                            case 0: Mem.AreEqual(10, relation.speed); break;
                            case 1: Mem.AreEqual(11, relation.speed); break;
                            case 2: Mem.AreEqual(12, relation.speed); break;
                        }
                    }
                    Mem.AreEqual(3, relationCount);
                    break;
            }
        }
        Mem.AreEqual(6, count);
        Mem.AssertNoAlloc(start);
        
        // --- test with additional filter condition 
        query.WithoutAnyComponents(ComponentTypes.Get<Position>());
        AreEqual(3, query.Count);
        count = 0;
        foreach (var entity in query.Entities) {
            var relations = entity.GetRelations<AttackRelation>();
            switch (count++) {
                case 0: AreEqual("{ 4 }",       relations.Debug());  break;
                case 1: AreEqual("{ 4, 5 }",    relations.Debug());  break;
                case 2: AreEqual("{ 4, 5 }",    relations.Debug());  break;
            }
        }
        Mem.AreEqual(3, count);
    }
    
    [Test]
    public static void Test_Relations_ForAllEntityRelations_Perf()
    {
        //  #PC: Test_Relations_ForAllEntityRelations_Perf - entities: 1000000  relationsPerEntity: 10  duration: 30 ms
        int entityCount         = 100;
        int relationsPerEntity  = 10;
        var store           = new EntityStore();
        var type            = store.GetArchetype(default);
        var createdEntities = type.CreateEntities(entityCount);
        foreach (var entity in createdEntities) {
            for (int n = 0; n < relationsPerEntity; n++) {
                entity.AddRelation(new IntRelation { value = n });
            }
        }
        int count = 0;
        var sw = new Stopwatch();
        sw.Start();
        store.ForAllEntityRelations((ref IntRelation relation, Entity entity) => {
            count++;
        });
        AreEqual(entityCount * relationsPerEntity, count);
        Console.WriteLine($"Test_Relations_ForAllEntityRelations_Perf - entities: {entityCount}  relationsPerEntity: {relationsPerEntity}  duration: {sw.ElapsedMilliseconds} ms");
    }
    
    /// Most efficient way to iterate all entity relations
    [Test]
    public static void Test_Relations_GetAllEntityRelations_Perf()
    {
        //  #PC: Test_Relations_GetAllEntityRelations_Perf - entities: 1000000  relationsPerEntity: 10  duration: 6 ms
        int entityCount         = 100;
        int relationsPerEntity  = 10;
        var store           = new EntityStore();
        var type            = store.GetArchetype(default);
        var createdEntities = type.CreateEntities(entityCount);
        foreach (var entity in createdEntities) {
            for (int n = 0; n < relationsPerEntity; n++) {
                entity.AddRelation(new IntRelation { value = n });
            }
        }
        int count = 0;
        var sw = new Stopwatch();
        sw.Start();
        var (entities, relations) = store.GetAllEntityRelations<IntRelation>();
        int length = entities.Count;
        for (int n = 0; n < length; n++) {
            count++;
            _ = entities[n];
            _ = relations[n];
        }
        AreEqual(entityCount * relationsPerEntity, count);
        Console.WriteLine($"Test_Relations_GetAllEntityRelations_Perf - entities: {entityCount}  relationsPerEntity: {relationsPerEntity}  duration: {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_Relations_query_exception()
    {
        var store    = new EntityStore();
        var e = Throws<InvalidOperationException>(() => {
            store.Query<AttackRelation, Position>();
        });
        AreEqual("relation component query cannot have other query components", e!.Message);
    }

#pragma warning disable CS0618 // Type or member is obsolete

    // [Test]
    public static void Test_Relations_Entity_References()
    {
        var store   = new EntityStore();
        var entity1  = store.CreateEntity();
        var entity2  = store.CreateEntity();
        var entity3  = store.CreateEntity();
        
        AreEqual(0, entity1.References.Count);
        
        entity2.AddComponent(new AttackComponent { target = entity1 });
        AreEqual(1, entity1.References.Count);
        
        entity2.AddRelation(new AttackRelation { target = entity1 });
        AreEqual(2, entity1.References.Count);
        
        entity3.AddRelation(new AttackRelation { target = entity1 });
        AreEqual(3, entity1.References.Count);
        
        int count = 0;
        foreach (var component in entity1.References) {
            switch (count++) {
                case 0: AreEqual(new AttackComponent { target = entity1 }, component.Value);    break;

                case 1: AreEqual(new AttackRelation  { target = entity1 }, component.Value);    break;
                case 2: AreEqual(new AttackRelation  { target = entity1 }, component.Value);    break;
            }
        }
        AreEqual(3, count);
    }
}

}
