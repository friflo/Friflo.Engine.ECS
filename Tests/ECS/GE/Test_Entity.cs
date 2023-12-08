using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Friflo.Fliox.Engine.ECS.EntityUtils;

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
        
        AddNewEntityScript(entity, script1Type);
        var script1     = GetEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        AreSame(typeof(TestScript1),    script1.GetType());
        
        var script2 = new TestScript2();
        AddEntityScript(entity, script2);
        var script2Result = GetEntityScript(entity, script2Type);
        AreSame(script2, script2Result);
        AreEqual(2,                     entity.Scripts.Length);
        
        // --- remove script1
        RemoveEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        // remove same script type again
        RemoveEntityScript(entity, script1Type);
        AreEqual(1,                     entity.Scripts.Length);
        
        // --- remove script2
        RemoveEntityScript(entity, script2Type);
        AreEqual(0,                     entity.Scripts.Length);
        // remove same script type again
        RemoveEntityScript(entity, script1Type);
        AreEqual(0,                     entity.Scripts.Length);
    }
    
    [Test]
    public static void Test_Entity_non_generic_Component_methods()
    {
        var store           = new EntityStore();
        var entity          = store.CreateEntity();
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(EntityName)];
        
        AddEntityComponent(entity, componentType);
        var component = GetEntityComponent(entity, componentType);
        AreEqual(1,                     entity.Archetype.ComponentCount);
        AreSame(typeof(EntityName),     component.GetType());
        
        RemoveEntityComponent(entity, componentType);
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
        
        IsFalse (entity1 == entity2);
        IsTrue  (entity1 != entity2);
        
        var comparer = EqualityComparer;
        IsTrue  (comparer.Equals(entity1, entity1));
        IsFalse (comparer.Equals(entity1, entity2));
        
        AreEqual(1, comparer.GetHashCode(entity1));
        AreEqual(2, comparer.GetHashCode(entity2));
        
        var e = Throws<NotImplementedException>(() => {
            _ = entity1.GetHashCode();
        });
        AreEqual("to avoid excessive boxing. Use: Id or Entity.EqualityComparer. id: 1", e!.Message);
        
        e = Throws<NotImplementedException>(() => {
            _ = entity1.Equals(entity2);
        });
        AreEqual("to avoid excessive boxing. Use: == or Entity.EqualityComparer. id: 1", e!.Message);
    }
    
    
    [Test]
    public static void Test_EntityStore_CreateEntity_Perf() 
    {
        var store = new EntityStore(PidType.UsePidAsId);
        for (int n = 0; n < 10_000_000; n++) {
            store.CreateEntity();
        }
        Console.WriteLine(store.EntityCount);
    }
    
    private static void Resize<T>(ref T[] array, int len) {
        var newArray = new T[len];
        if (array != null) {
            Array.Copy(array, newArray, array.Length);
        }
        array = newArray;
    }
    
    [Test]
    public static void Test_EntityStore_Resize() 
    {
        int count = 0;
        for (int i = 0; i < 100; i++)
        {
            var array = new Position[10];
            while (array.Length < 10_000_000) {
                var newLen = array.Length * 2;
                Resize(ref array, newLen);
                count++;
            }
        }
        Console.WriteLine(count);
    }
}




