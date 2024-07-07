using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable UnusedVariable
// ReSharper disable AccessToModifiedClosure
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Relations {

public static class Test_Relations
{
    [Test]
    public static void Test_Relations_enum_relation()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Axe,       amount = 1 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Gun,       amount = 2 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Sword,     amount = 3 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Shield,    amount = 4 });
        var inventoryItems = entity.GetRelations<InventoryItem>();
        AreEqual("{ Axe, Gun, Sword, Shield }", inventoryItems.Debug());
        
        var start = Mem.GetAllocatedBytes();
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Axe,       amount = 11 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Gun,       amount = 12 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Sword,     amount = 13 });
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Shield,    amount = 14 });
        inventoryItems = entity.GetRelations<InventoryItem>();
        Mem.AssertNoAlloc(start);
        AreEqual("{ Axe, Gun, Sword, Shield }", inventoryItems.Debug());
    }
    
    [Test]
    public static void Test_Relations_GetRelation()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        InventoryItem value;
        
        var knf = Throws<KeyNotFoundException>(() => {
            entity.GetRelation<InventoryItem, InventoryItemType>(InventoryItemType.Sword);
        });
        AreEqual("relation not found. key 'Sword' id: 1", knf!.Message);
        
        IsFalse (entity.TryGetRelation(InventoryItemType.Axe,    out value));
        AreEqual(new InventoryItem(), value);
        
        entity.AddRelation(new InventoryItem { type = InventoryItemType.Axe,       amount = 1 });
        entity.GetRelation<InventoryItem, InventoryItemType>(InventoryItemType.Axe); // force one time allocation
        
        var start = Mem.GetAllocatedBytes();
        
        ref var axeRelation = ref entity.GetRelation<InventoryItem, InventoryItemType>(InventoryItemType.Axe);
        Mem.AreEqual(1, axeRelation.amount);
        
        Mem.IsTrue  (entity.TryGetRelation(InventoryItemType.Axe,    out value));
        Mem.AreEqual(1, value.amount);
        
        Mem.IsFalse (entity.TryGetRelation(InventoryItemType.Shield, out value));
        Mem.AreEqual(0, (int)value.type);
        
        Mem.AssertNoAlloc(start);
        
        knf = Throws<KeyNotFoundException>(() => {
            entity.GetRelation<InventoryItem, InventoryItemType>(InventoryItemType.Shield);
        });
        AreEqual("relation not found. key 'Shield' id: 1", knf!.Message);
    }
    
    
    [Test]
    public static void Test_Relations_add_remove()
    {
        var store       = new EntityStore();
        var entity3     = store.CreateEntity(3);
            
        var target10    = store.CreateEntity(10);
        var target11    = store.CreateEntity(11);
        var components  = entity3.Components;
        IsTrue (entity3.AddRelation(new AttackRelation { target = target10, speed = 1 }));
        AreEqual("Components: [] +1 relations", components.ToString());
        IsTrue (entity3.AddRelation(new AttackRelation { target = target11, speed = 1  }));
        AreEqual("Components: [] +2 relations", components.ToString());
        IsFalse(entity3.AddRelation(new AttackRelation { target = target11, speed = 42  }));
        AreEqual("Components: [] +2 relations", components.ToString());
        
        IsTrue (entity3.RemoveRelation<AttackRelation>(target10));
        AreEqual("Components: [] +1 relations", components.ToString());
        IsFalse(entity3.RemoveRelation<AttackRelation>(target10));
        AreEqual("Components: [] +1 relations", components.ToString());
        
        IsTrue (entity3.RemoveRelation<AttackRelation>(target11));
        AreEqual("Components: []", components.ToString());
        IsFalse(entity3.RemoveRelation<AttackRelation>(target11));
        AreEqual("Components: []", components.ToString());
        
        var start = Mem.GetAllocatedBytes();
        entity3.AddRelation(new AttackRelation { target = target10, speed = 1 });
        entity3.AddRelation(new AttackRelation { target = target11, speed = 1 });
        entity3.RemoveRelation<AttackRelation>(target11);
        entity3.RemoveRelation<AttackRelation>(target10);
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Relations_add_remove_default()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        IsTrue  (entity.AddRelation<AttackRelation>(default));
        IsFalse (entity.AddRelation<AttackRelation>(default));
        var relations = entity.GetRelations<AttackRelation>();
        AreEqual("{ 0 }", relations.Debug());
        IsTrue  (relations[0].target == default);
        IsTrue  (entity.RemoveRelation<AttackRelation>(default));
        IsFalse (entity.RemoveRelation<AttackRelation>(default));
        relations = entity.GetRelations<AttackRelation>();
        AreEqual("{ }", relations.Debug());
    }    
    
#pragma warning disable CS0618 // Type or member is obsolete
    [Test]
    public static void Test_Relations_EntityComponents()
    {
        var store    = new EntityStore();
        var target10 = store.CreateEntity(10);
        var target11 = store.CreateEntity(11);
        var entity   = store.CreateEntity(1);

        var components = entity.Components;
        entity.AddComponent(new Position());
        AreEqual(1, components.Count);

        entity.AddRelation(new AttackRelation { target = target10, speed = 20 });
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
        
        entity.AddRelation(new AttackRelation { target = target11, speed = 21 });
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
    public static void Test_Relations_Enumerator()
    {
        var store    = new EntityStore();
        var entity  = store.CreateEntity(1);

        var target10 = store.CreateEntity(10);
        var target11 = store.CreateEntity(11);
        entity.AddRelation(new AttackRelation { target = target10, speed = 20 });
        entity.AddRelation(new AttackRelation { target = target11, speed = 21 });
        
        var relations = entity.GetRelations<AttackRelation>();
        
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
        enumerable2  = entity2.GetRelations<AttackRelation>();
        count = 0;
        foreach (var relation in enumerable2) {
            count++;
        }
        AreEqual(0, count);
    }
    
    [Test]
    public static void Test_Relations_EntityReadOnlyCollection()
    {
        var store   = new EntityStore();
        var entities = store.GetAllEntitiesWithRelations<IntRelation>();

        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddRelation(new IntRelation { value = 10 });
        entity2.AddRelation(new IntRelation { value = 20 });
        
        AreEqual(2, entities.Count);
        AreEqual("Entity[2]", entities.ToString());
        {
            int count = 0;
            foreach (var entity in entities) {
                switch (count++) {
                    case 0: AreEqual(1, entity.Id); break;
                    case 1: AreEqual(2, entity.Id); break;
                }
            }
            AreEqual(2, count);
        } {
            int count = 0;
            IEnumerable enumerable = entities;
            IEnumerator enumerator = enumerable.GetEnumerator();
            using var enumerator1 = enumerator as IDisposable;
            enumerator.Reset();
            while (enumerator.MoveNext()) {
                var entity = (Entity)enumerator.Current!;
                switch (count++) {
                    case 0: AreEqual(1, entity.Id); break;
                    case 1: AreEqual(2, entity.Id); break;
                }
            }
            AreEqual(2, count);
        } {
            int count = 0;
            IEnumerable<Entity> enumerable = entities;
            foreach (var entity in enumerable) {
                count++; 
            }
            AreEqual(2, count);
        }
    }
    
    [Test]
    public static void Test_Relations_adjust_position_on_remove_relation()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        entity1.AddRelation(new IntRelation { value = 1 });
        entity1.AddRelation(new IntRelation { value = 2 });
        // last relation IntRelation { value = 2 } at StructHeap<>.components[1] is moved to components[0]
        // So its stored position need to be updated 
        entity1.RemoveRelation<IntRelation, int>(1);
        entity2.AddRelation(new IntRelation { value = 3 });
        
        AreEqual("{ 2 }",                       entity1.GetRelations<IntRelation>().Debug());
        AreEqual("{ 3 }",                       entity2.GetRelations<IntRelation>().Debug());
        AreEqual("Relations<IntRelation>[1]",   entity1.GetRelations<IntRelation>().ToString());
    }
    
    [Test]
    public static void Test_Relations_int_relation()
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
                Mem.AreEqual(n, entity.GetRelations<IntRelation>().Length);
                Mem.IsTrue(entity.AddRelation(new IntRelation{ value = n }));
            }
        }
        foreach (var entity in entities) {
            for (int n = 0; n < relationCount; n++) {
                Mem.AreEqual(relationCount - n, entity.GetRelations<IntRelation>().Length);
                Mem.IsTrue(entity.RemoveRelation<IntRelation, int>(n));
            }
        }
        var start = Mem.GetAllocatedBytes();
        foreach (var entity in entities) {
            for (int n = 0; n < relationCount; n++) {
                Mem.IsTrue(entity.AddRelation(new IntRelation{ value = n }));
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
    
    /// null is a valid relation key
    [Test]
    public static void Test_Relations_null_relation_key()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddRelation(new StringRelation { value = null });
        AreEqual("{  }", entity.GetRelations<StringRelation>().Debug());
        IsNull  (entity.GetRelation<StringRelation, string>(null).value);
    }
    
    [Test]
    public static void Test_Relations_Entity_Component_methods()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        IsFalse(entity.HasComponent<IntRelation>());
        
        IsFalse(entity.TryGetComponent<IntRelation>(out var relation));
        AreEqual(new IntRelation(), relation);
        
        Throws<NullReferenceException>(() => {
            entity.GetComponent<IntRelation>();
        });

        var e = Throws<ArgumentException>(() => {
            entity.RemoveComponent<IntRelation>();
        });
        AreEqual("relation component must be removed with:  entity.RemoveRelation<IntRelation,Int32>(key);  id: 1", e!.Message);
        entity.RemoveRelation<IntRelation,int>(42); // example
        
        e = Throws<ArgumentException>(() => {
            entity.AddComponent(new IntRelation());
        });
        AreEqual("relation component must be added with:  entity.AddRelation(new IntRelation());  id: 1", e!.Message);
        entity.AddRelation(new IntRelation()); // example
        
    }
    
    [Test]
    public static void Test_Relations_NullReferenceExceptions()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.DeleteEntity();
        var expect = "entity is null. id: 1";
        
        var nre = Throws<NullReferenceException>(() => {
            entity.GetRelations<IntRelation>();
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.GetRelation<IntRelation, int>(1);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.TryGetRelation<IntRelation, int>(1, out _);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.RemoveRelation<IntRelation, int>(1);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.RemoveRelation<AttackRelation>(default);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.GetIncomingLinks<AttackRelation>();
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.AddRelation(new AttackRelation());
        });
        AreEqual(expect, nre!.Message);
    }
}

}
