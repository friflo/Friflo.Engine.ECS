using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_Entity
{
    [Test]
    public static void Test_Entity_new_EntityStore_Perf()
    {
        long count = 10; // 10_000_000 ~ #PC: 4867 ms
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            _ = new EntityStore(PidType.UsePidAsId);
        }
        Console.WriteLine($"new EntityStore() - duration: {sw.ElapsedMilliseconds}");
    }
    
    [Test]
    public static void Test_Entity_non_generic_Script_methods()
    {
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        var schema      = EntityStore.GetEntitySchema();
        var script1Type = schema.ScriptTypeByType[typeof(TestScript1)];
        var script2Type = schema.ScriptTypeByType[typeof(TestScript2)];
        
        EntityUtils.AddNewEntityScript(entity, script1Type);
        var script1     = EntityUtils.GetEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        AreSame(typeof(TestScript1),    script1.GetType());
        
        var script2 = new TestScript2();
        EntityUtils.AddEntityScript(entity, script2);
        var script2Result = EntityUtils.GetEntityScript(entity, script2Type);
        AreSame(script2, script2Result);
        AreEqual(2,                     entity.Scripts.Length);
        
        // --- remove script1
        EntityUtils.RemoveEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        // remove same script type again
        EntityUtils.RemoveEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        
        // --- remove script2
        EntityUtils.RemoveEntityScript(entity, script2Type);
        AreEqual(0,                     entity.Scripts.Length);
        // remove same script type again
        EntityUtils.RemoveEntityScript(entity, script1Type);
        AreEqual(0,                     entity.Scripts.Length);
    }
    
    [Test]
    public static void Test_Entity_non_generic_Component_methods()
    {
        var store           = new EntityStore();
        var entity          = store.CreateEntity();
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(EntityName)];
        
        EntityUtils.AddEntityComponent(entity, componentType);
        AreEqual(1,                     entity.Archetype.ComponentCount);
        var component = EntityUtils.GetEntityComponent(entity, componentType);
        AreSame(typeof(EntityName),     component.GetType());
        
        EntityUtils.RemoveEntityComponent(entity, componentType);
        AreEqual(0,                     entity.Archetype.ComponentCount);
        
        EntityUtils.AddEntityComponentValue(entity, componentType, new EntityName("comp-value"));
        AreEqual(1,                     entity.Archetype.ComponentCount);
        component = EntityUtils.GetEntityComponent(entity, componentType);
        var name = (EntityName)component;
        AreEqual("comp-value", name.value);
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
        var script1     = new TestScript1();
        entity.AddScript(script1);
        entity.AddComponent(new EntityName("original"));
        entity.AddTag<TestTag>();
        
        // --- clone entity with blittable components & scripts
        var clone = store.CloneEntity(entity);
        
        AreEqual("Tags: [#TestTag]",            clone.Tags.ToString());
        AreEqual("Components: [EntityName]",    clone.Components.ToString());
        AreEqual(1,                             clone.Scripts.Length);
        NotNull(clone.GetScript<TestScript1>());
        AreNotSame(script1,                     clone.Scripts[0]);
        
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
    
    [Test]
    public static void Test_Entity_EqualityComparer()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        
        var comparer = EntityUtils.EqualityComparer;
        IsTrue  (comparer.Equals(entity1, entity1));
        IsFalse (comparer.Equals(entity1, entity2));
        
        AreEqual(1, comparer.GetHashCode(entity1));
        AreEqual(2, comparer.GetHashCode(entity2));
    }
    
    [Test]
    public static void Test_Entity_Equality()
    {
        var store       = new EntityStore();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        
        // --- operator ==, !=
        IsFalse (entity1 == entity2);
        IsTrue  (entity1 != entity2);
        
        // --- IEquatable<Entity>
        var start = Mem.GetAllocatedBytes();
        Mem.AreEqual (false, entity1.Equals(entity2));
        Mem.AreEqual (true,  entity1.Equals(entity1));
        Mem.AssertNoAlloc(start);
        
        // --- object.GetHashCode()
        var e = Throws<NotImplementedException>(() => {
            _ = entity1.GetHashCode();
        });
        AreEqual("to avoid excessive boxing. Use Id or EntityUtils.EqualityComparer. id: 1", e!.Message);
        
        // --- object.Equals()
        e = Throws<NotImplementedException>(() => {
            object obj = entity1;
            _ = obj.Equals(entity2);
        });
        AreEqual("to avoid excessive boxing. Use == Equals(Entity) or EntityUtils.EqualityComparer. id: 1", e!.Message);
    }
    
    [Test]
    public static void Test_Entity_Enabled()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);
        
        IsTrue (entity.Enabled);
        
        entity.Enabled = false;
        IsFalse(entity.Enabled);
        
        entity.Enabled = true;
        IsTrue (entity.Enabled);
    }
    
    [Test]
    public static void Test_Entity_EnableTree()
    {
        var count       = 10;    // 1_000_000 ~ #PC: 8296 ms
        var entityCount = 100;
        var store       = new EntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity();
        var arch2       = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        var arch3       = store.GetArchetype(ComponentTypes.Get<Position, Rotation>(), Tags.Get<Disabled>());
        
        for (int n = 1; n < entityCount; n++) {
            root.AddChild(arch2.CreateEntity());
        }
        IsTrue (root.Enabled);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0;
        for (int i = 0; i < count; i++)
        {
            root.EnableTree();
            root.DisableTree();
            if (i == 0) start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"Disable / Enable - duration: {sw.ElapsedMilliseconds} ms");
        
        var query       = store.Query();
        AreEqual(0,                 query.Count);
        
        var disabled    = store.Query().WithDisabled();
        AreEqual(entityCount,       disabled.Count);
        
        AreEqual(entityCount,       store.Count);
        AreEqual(0,                 arch2.Count);
        AreEqual(entityCount - 1,   arch3.Count);
        IsFalse (root.Enabled);
    }
}

}
