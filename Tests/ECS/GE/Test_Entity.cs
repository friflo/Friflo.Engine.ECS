using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;


public static class Test_Entity
{
    [Test]
    public static void Test_Entity_non_generic_Script_methods()
    {
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        var schema      = EntityStore.GetEntitySchema();
        var script1Type = schema.ScriptTypeByType[typeof(TestScript1)];
        var script2Type = schema.ScriptTypeByType[typeof(TestScript2)];
        
        Entity.AddNewEntityScript(entity, script1Type);
        var script1     = Entity.GetEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        AreSame(typeof(TestScript1),    script1.GetType());
        
        var script2 = new TestScript2();
        Entity.AddEntityScript(entity, script2);
        var script2Result = Entity.GetEntityScript(entity, script2Type);
        AreSame(script2, script2Result);
        AreEqual(2,                     entity.Scripts.Length);
        
        // --- remove script1
        Entity.RemoveEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        // remove same script type again
        Entity.RemoveEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        
        // --- remove script2
        Entity.RemoveEntityScript(entity, script2Type);
        AreEqual(0,                     entity.Scripts.Length);
        // remove same script type again
        Entity.RemoveEntityScript(entity, script1Type);
        AreEqual(0,                     entity.Scripts.Length);
    }
    
    [Test]
    public static void Test_Entity_non_generic_Component_methods()
    {
        var store           = new EntityStore();
        var entity          = store.CreateEntity();
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(EntityName)];
        
        Entity.AddEntityComponent(entity, componentType);
        var component = Entity.GetEntityComponent(entity, componentType);
        AreEqual(1,                     entity.Archetype.ComponentCount);
        AreSame(typeof(EntityName),     component.GetType());
        
        Entity.RemoveEntityComponent(entity, componentType);
        AreEqual(0,                     entity.Archetype.ComponentCount);
    }
    
    

    [Test]
    public static void Test_Entity_TryGetEntityByPid()
    {
        var store   = new EntityStore(PidType.RandomPids);
        Assert_TryGetEntityByPid(store);
        
        store       = new EntityStore(PidType.UsePidAsId);
        Assert_TryGetEntityByPid(store);
    }
    
    private static void Assert_TryGetEntityByPid(EntityStore store)
    {
        var entity2 = store.CreateEntity(2);
        Entity entity;
        
        IsTrue (store.TryGetEntityByPid(entity2.Pid, out entity));
        IsTrue(!entity.IsNull);
        
        IsFalse(store.TryGetEntityByPid( 0, out entity));
        IsTrue(entity.IsNull);
        
        IsFalse(store.TryGetEntityByPid(-1, out entity));
        IsTrue(entity.IsNull);
        
        IsFalse(store.TryGetEntityByPid( 1, out entity));
        IsTrue(entity.IsNull);
        
        IsFalse(store.TryGetEntityByPid( 3, out entity));
        IsTrue(entity.IsNull);
        
        IsFalse(store.TryGetEntityByPid(long.MaxValue, out entity));
        IsTrue(entity.IsNull);
    }
        
    [Test]
    public static void Test_EntityStore_CloneEntity()
    {
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        entity.AddScript(new TestScript1());
        entity.AddComponent(new EntityName("original"));
        entity.AddTag<TestTag>();
        
        // --- clone entity with blittable components & scripts
        var clone = store.CloneEntity(entity);
        
        AreEqual("Tags: [#TestTag]",            clone.Tags.ToString());
        AreEqual("Components: [EntityName]",    clone.Components.ToString());
        AreEqual(1,                             clone.Scripts.Length);
        NotNull(clone.GetScript<TestScript1>());
        
        // --- clone entity with non blittable component
        entity.AddComponent<NonBlittableArray>();
        clone = store.CloneEntity(entity);
        AreEqual("Components: [EntityName, NonBlittableArray]",    clone.Components.ToString());
        
        // --- clone entity with non blittable script
        entity.RemoveComponent<NonBlittableArray>();
        entity.AddScript(new NonBlittableScript());
        clone = store.CloneEntity(entity);
        
        AreEqual(2,                             clone.Scripts.Length);
        NotNull(clone.GetScript<NonBlittableScript>());
    }
}




