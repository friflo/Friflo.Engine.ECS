using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_Entity_NRE
{
    [Test]
    public static void Test_Entity_null()
    {
        Entity entity = default;
        IsNull(entity.Archetype);
        IsNull(entity.Store);
        IsTrue(entity.IsNull);
    }
    
    [Test]
    public static void Test_Entity_NRE_DeleteEntity()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.DeleteEntity();
        var expect = "entity is null. id: 1";
        
        var nre = Throws<NullReferenceException>(() => {
            entity.DeleteEntity();
        });
        AreEqual(expect, nre!.Message);
    }
    
    [Test]
    public static void Test_Entity_NRE_Component()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.DeleteEntity();
        var expect      = "entity is null. id: 1";
        var expectArg   = "entity is null. id: 1 (Parameter 'entity')";
        
        // --- components
        var nre = Throws<NullReferenceException>(() => {
            entity.GetComponent<Position>();
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.TryGetComponent<Position>(out _);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.AddComponent(new Position());
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.AddComponent<Position>();
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.RemoveComponent<Position>();
        });
        AreEqual(expect, nre!.Message);
        
        var schema = EntityStore.GetEntitySchema();
        var componentType = schema.ComponentTypeByType[typeof(Position)];
        
        nre = Throws<NullReferenceException>(() => {
            EntityUtils.RemoveEntityComponent(entity, componentType);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            EntityUtils.AddEntityComponent(entity, componentType);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            EntityUtils.AddEntityComponentValue(entity, componentType, new Position());
        });
        AreEqual(expect, nre!.Message);
        
        // --- tags
        nre = Throws<NullReferenceException>(() => {
            entity.AddTag<TestTag>();
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.AddTags(default);
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.RemoveTag<TestTag>();
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            entity.RemoveTags(default);
        });
        AreEqual(expect, nre!.Message);
        
        var ane = Throws<ArgumentException>(() => {
            store.CloneEntity(entity);
        });
        AreEqual(expectArg, ane!.Message);
    }
    
    [Test]
    public static void Test_Entity_NRE_built_in_components()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.DeleteEntity();
        var expect = "entity is null. id: 1";
        
        // --- components
        var nre = Throws<NullReferenceException>(() => {
            _ = entity.Name;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.Position;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.Rotation;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.Scale3;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.HasName;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.HasPosition;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.HasRotation;
        });
        AreEqual(expect, nre!.Message);
        
        nre = Throws<NullReferenceException>(() => {
            _ = entity.HasScale3;
        });
        AreEqual(expect, nre!.Message);

    }
    
    [Test]
    public static void Test_Entity_NRE_misc() {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.DeleteEntity();
        var expect = "entity is null. id: 1";
        
        var nre = Throws<NullReferenceException>(() => {
            _ = entity.ChildCount;
        });
        AreEqual(expect, nre!.Message);
    }
}

}
