using System;
using System.Diagnostics;
using System.Linq;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable EqualExpressionComparison
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
    public static void Test_Entity_CapacitySumArchetypes()
    {
        var store = new EntityStore();
        for (int n = 0; n < 10000; n++) {
            var entity = store.CreateEntity();
            entity.AddComponent<Position>();
        }
        AreEqual(16896, store.CapacitySumArchetypes);
        
        store.ShrinkRatioThreshold = 0;
        var entities = store.Entities.ToArray();
        foreach (var entity in entities) {
            entity.DeleteEntity();
        }
        AreEqual(1536, store.CapacitySumArchetypes);
    }
    
    [Test]
    public static void Test_Entity_non_generic_Script_methods()
    {
        var store       = new EntityStore(PidType.RandomPids);
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
        var store           = new EntityStore(PidType.RandomPids);
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
    public static void Assert_GetEntityById()
    {
        var store   = new EntityStore(PidType.RandomPids);
        store.CreateEntity(2);

        AreEqual(128, store.Capacity);
        
        IsTrue (store.GetEntityById(0).IsNull);
        IsTrue (store.GetEntityById(1).IsNull);
        IsFalse(store.GetEntityById(2).IsNull);
        IsTrue (store.GetEntityById(3).IsNull);
        
        var e = Throws<ArgumentException>(() => {
            store.GetEntityById(-1);
        });
        AreEqual("id: -1. expect in [0, current max id: 127]", e!.Message);
        e = Throws<ArgumentException>(() => {
            store.GetEntityById(128);
        });
        AreEqual("id: 128. expect in [0, current max id: 127]", e!.Message);
    }
    
    [Test]
    public static void Assert_GetEntityById_Perf() {
        var count = 10; // 10_000_000_000L;
        // Assert_GetEntityById_Perf() - count: 10000000000, duration: 3238
        var store   = new EntityStore();
        store.CreateEntity(2);
        var sw = new Stopwatch();
        sw.Start();
        for (long n = 0; n < count; n++) {
            store.GetEntityById(2);
        }
        Console.WriteLine($"Assert_GetEntityById_Perf() - count: {count}, duration: {sw.ElapsedMilliseconds}");
    }
    
    [Test]
    public static void Assert_TryGetEntityById()
    {
        var store   = new EntityStore(PidType.RandomPids);
        store.CreateEntity(2);

        AreEqual(128, store.Capacity);
        
        IsFalse(store.TryGetEntityById(-1, out Entity entity));
        IsTrue (entity.IsNull);
        
        IsFalse(store.TryGetEntityById(0, out entity));
        IsTrue (entity.IsNull);
        
        IsFalse(store.TryGetEntityById(1, out entity));
        IsTrue (entity.IsNull);
        
        IsTrue (store.TryGetEntityById(2, out entity));
        IsFalse(entity.IsNull);

        IsFalse(store.TryGetEntityById(3, out entity));
        IsTrue (entity.IsNull);
        
        IsFalse(store.TryGetEntityById(4, out entity));
        IsTrue (entity.IsNull);
    }
     
    [Test]
    public static void Test_EntityStore_CloneEntity()
    {
        var store       = new EntityStore(PidType.RandomPids);
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
        var store       = new EntityStore(PidType.RandomPids);
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
        var store       = new EntityStore(PidType.RandomPids);
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        
        // --- operator ==, !=
        IsFalse (entity1 == entity2);
        IsTrue  (entity1 != entity2);
        
#pragma warning disable CS1718  // Comparison made to same variable; did you mean to compare something else?
        IsFalse (entity1 != entity1);
#pragma warning restore CS1718
        
        // --- IEquatable<Entity>
        Mem.AreEqual (false, entity1.Equals(entity2)); // force one time allocation
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
    
    [Test]
    public static void Test_Entity_create_delete_Entity_events()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var createCount = 0;
        Action<EntityCreate> createHandler = create => {
            var str = create.ToString();
            switch (createCount++) {
                case 0:     AreSame (store,         create.Store);
                            AreEqual("id: 1  []",   create.Entity.ToString());
                            AreEqual("entity: 1 - event > EntityCreate", str);      break;
                case 1:     AreEqual("entity: 10 - event > EntityCreate", str);      break;
                case 2:     AreEqual("entity: 2 - event > EntityCreate", str);      break;
                case 3:     AreEqual("entity: 11 - event > EntityCreate", str);      break;
                case 4:     AreEqual("entity: 3 - event > EntityCreate", str);      break;
                default: throw new InvalidOperationException("unexpected");
            }
        };
        var deleteCount = 0;
        Action<EntityDelete> deleteHandler = delete  => {
            var str = delete.ToString();
            switch (deleteCount++) {
                case 0:
                    AreSame (store, delete.Store);
                    AreEqual("id: 1  []",                           delete.Entity.ToString());
                    AreEqual("entity: 1 - event > EntityDelete",    str);
                    break;
                case 1:
                    // Important: entity components are still present
                    AreEqual("id: 2  [EntityName]",                 delete.Entity.ToString());
                    break;
                default: throw new InvalidOperationException("unexpected");
            } 
        };
        store.OnEntityCreate += createHandler;
        store.OnEntityDelete += deleteHandler;
            
        var entity1 = store.CreateEntity();
        store.CreateEntity(10);
        
        var arch    = store.GetArchetype(ComponentTypes.Get<EntityName>());
        var entity2 = arch.CreateEntity();
        arch.CreateEntity(11);
        
        var clone = store.CloneEntity(entity1);
        
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        store.OnEntityCreate -= createHandler;
        store.OnEntityDelete -= deleteHandler;
        
        store.CreateEntity();   // does not fire event - handler removed
        clone.DeleteEntity(); // does not fire event - handler removed
        
        AreEqual(5, createCount);
        AreEqual(2, deleteCount);
    }
    
    
    [Test]
    public static void Test_Entity_delete_entity_event_exception()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var deleteCount = 0;
        store.OnEntityDelete += delete  => {
            deleteCount++;
            // Important: entity components are still present
            AreEqual("id: 1  \"test\"  [EntityName]",   delete.Entity.ToString());
            throw new InvalidOperationException("test delete exception");
        };
        var entity  = store.CreateEntity();
        entity.AddComponent(new EntityName("test"));
        
        var e = Throws<InvalidOperationException>(() => {
            entity.DeleteEntity();    
        });
        AreEqual("test delete exception", e!.Message);
        AreEqual(0, store.Count);
        AreEqual(1, deleteCount);
    }
    
    [Test]
    public static void Test_Entity_CreateEntity_Perf()
    {
        int count   = 10; // 10_000_000 ~ #PC: 316 ms
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        var capacity = store.Capacity;
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            store.CreateEntity();
        }
        Console.WriteLine($"CreateEntity(PidType.UsePidAsId) - count: {count}, duration: {sw.ElapsedMilliseconds}");
        AreEqual(count,     store.Count);
        AreEqual(capacity,  store.Capacity);
    }
    
    [Test]
    public static void Test_Entity_DeleteEntity_Perf()
    {
        int count   = 10; // 10_000_000 ~ #PC: 309 ms
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        for (int n = 1; n <= count; n++) {
            store.CreateEntity(n);
        }
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 1; n <= count; n++) {
            var entity = store.GetEntityById(n);
            entity.DeleteEntity();
        }
        Console.WriteLine($"DeleteEntity() - count: {count}, duration: {sw.ElapsedMilliseconds}");
        AreEqual(0,     store.Count);
    }
   
    [Test]
    public static void Test_Entity_CreateEntity_generic()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        int entityCount = 0;
        store.OnEntityCreate += create => {
            var str = create.Entity.ToString();
            switch (entityCount++) {
                case 0: AreEqual("id: 1  [#TestTag]",                                                           str); break;
                case 1: AreEqual("id: 2  [Position, #TestTag]",                                                 str); break;
                case 2: AreEqual("id: 3  [Position, Rotation, #TestTag]",                                       str); break;
                case 3: AreEqual("id: 4  [Position, Rotation, Scale3, #TestTag]",                               str); break;
                case 4: AreEqual("id: 5  [Position, Rotation, Scale3, MyComponent1, #TestTag]",                 str); break;
                case 5: AreEqual("id: 6  [Position, Rotation, Scale3, MyComponent1, MyComponent2, #TestTag]",   str); break;
            }
        };
        int tagCount = 0;
        store.OnTagsChanged += changed => {
            tagCount++;
            AreEqual("Tags: [#TestTag]", changed.AddedTags.ToString());
        };
        int componentCount = 0;
        store.OnComponentAdded += changed => {
            var str = changed.ToString();
            switch (componentCount++) {
                // --- entity 2
                case 0:     AreEqual("entity: 2 - event > Add Component: [Position]",       str); break;
                
                // --- entity 3
                case 1:     AreEqual("entity: 3 - event > Add Component: [Position]",       str); break;
                case 2:     AreEqual("entity: 3 - event > Add Component: [Rotation]",       str); break;
                
                // --- entity 4
                case 3:     AreEqual("entity: 4 - event > Add Component: [Position]",       str); break;
                case 4:     AreEqual("entity: 4 - event > Add Component: [Rotation]",       str); break;
                case 5:     AreEqual("entity: 4 - event > Add Component: [Scale3]",         str); break;
                
                // --- entity 5
                case 6:     AreEqual("entity: 5 - event > Add Component: [Position]",       str); break;
                case 7:     AreEqual("entity: 5 - event > Add Component: [Rotation]",       str); break;
                case 8:     AreEqual("entity: 5 - event > Add Component: [Scale3]",         str); break;
                case 9:     AreEqual("entity: 5 - event > Add Component: [MyComponent1]",   str); break;
                
                // --- entity 6
                case 10:    AreEqual("entity: 6 - event > Add Component: [Rotation]",       str); break;
                case 11:    AreEqual("entity: 6 - event > Add Component: [Position]",       str); break;
                case 12:    AreEqual("entity: 6 - event > Add Component: [Scale3]",         str); break;
                case 13:    AreEqual("entity: 6 - event > Add Component: [MyComponent1]",   str); break;
                case 14:    AreEqual("entity: 6 - event > Add Component: [MyComponent2]",   str); break;
            }
        };
        var tags    = Tags.Get<TestTag>();
        {
            var entity = store.CreateEntity(tags);
            IsTrue(entity.Tags.Has<TestTag>());
        } {
            var entity = store.CreateEntity(new Position(1,1,1), tags);
            IsTrue(entity.Tags.Has<TestTag>());
            AreEqual(1,         entity.Position.x);
        } {
            var entity = store.CreateEntity(new Position(1,1,1), new Rotation(1,1,1,1), tags);
            IsTrue(entity.Tags.Has<TestTag>());
            AreEqual(1,         entity.Position.x);
            AreEqual(1,         entity.Rotation.x);
        } {
            var entity = store.CreateEntity(new Position(1,1,1), new Rotation(1,1,1,1), new Scale3(1,1,1), tags);
            IsTrue(entity.Tags.Has<TestTag>());
            AreEqual(1,         entity.Position.x);
            AreEqual(1,         entity.Rotation.x);
            AreEqual(1,         entity.Scale3.x);
        } {
            var entity = store.CreateEntity(new Position(1,1,1), new Rotation(1,1,1,1), new Scale3(1,1,1), new MyComponent1 { a = 1} , tags);
            IsTrue(entity.Tags.Has<TestTag>());
            AreEqual(1,         entity.Position.x);
            AreEqual(1,         entity.Rotation.x);
            AreEqual(1,         entity.Scale3.x);
            AreEqual(1,         entity.GetComponent<MyComponent1>().a);
        } {
            var entity = store.CreateEntity(new Rotation(1,1,1,1), new Position(1,1,1), new Scale3(1,1,1), new MyComponent1 { a = 1}, new MyComponent2 { b = 1}, tags);
            IsTrue(entity.Tags.Has<TestTag>());
            AreEqual(1,         entity.Position.x);
            AreEqual(1,         entity.Rotation.x);
            AreEqual(1,         entity.Scale3.x);
            AreEqual(1,         entity.GetComponent<MyComponent1>().a);
            AreEqual(1,         entity.GetComponent<MyComponent2>().b);
        }
        AreEqual(6,  entityCount);
        AreEqual(6,  tagCount);
        AreEqual(15, componentCount);
    }
    
    [Test]
    public static void Test_Entity_CreateEntity_generic_6_and_more() 
    {
        var store = new EntityStore(PidType.UsePidAsId);
    
        var tagEventCount = 0;
        var componentEventCount = 0;
        var createEventCount = 0;
        
        store.OnEntityCreate    += _ => { createEventCount++; };
        store.OnTagsChanged     += _ => { tagEventCount++; };
        store.OnComponentAdded  += _ => { componentEventCount++; };
    
        var tag         = Tags.Get<TestTag>();

        store.CreateEntity(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), tag);
        store.CreateEntity(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), tag);
        store.CreateEntity(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), tag);
        store.CreateEntity(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6(), tag);
        store.CreateEntity(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6(), new MyComponent7(), tag);
    
        AreEqual(5,  createEventCount);
        AreEqual(5,  tagEventCount);
        AreEqual(40, componentEventCount);
    }
    
    
    [Test]
    public static void Test_Entity_CreateEntity_generic_Perf()
    {
        int count   = 10; // 10_000_000 ~ #PC: 792 ms
        var store   = new EntityStore(PidType.UsePidAsId);
        store.EnsureCapacity(count);
        var type = store.CreateEntity(new Position(), new EntityName(), new Rotation(), new Scale3(), new MyComponent1()).Archetype;
        type.EnsureCapacity(count);
        
        var capacity = store.Capacity;
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 1; n < count; n++) {
            store.CreateEntity(new Position(), new EntityName(), new Rotation(), new Scale3(), new MyComponent1());
        }
        Console.WriteLine($"CreateEntity(PidType.UsePidAsId) - count: {count}, duration: {sw.ElapsedMilliseconds}");
        AreEqual(count,     store.Count);
        AreEqual(count,     type.Count);
        AreEqual(capacity,  store.Capacity);
    }
}

}
