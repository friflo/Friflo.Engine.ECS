using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value

// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_StructComponent
{
    [Test]
    public static void Test_1_TryGetComponentValue() {
        var store   = new EntityStore(PidType.RandomPids);
        
        var player1 = store.CreateEntity();
        IsTrue(player1.AddComponent<Scale3>());
        
        var player2 = store.CreateEntity();
        var myComponent1 = new MyComponent1 { a = 1 };
        IsTrue(player2.AddComponent(myComponent1));
        
        var position = new Position { x = 2 };
        IsTrue(player2.AddComponent(position));

        var count = player2.Archetype.ComponentCount;
        AreEqual(2, count);
        
        var success = player2.TryGetComponent(out Position pos);
        IsTrue(success);
        AreEqual(2, pos.x);
        
        success = player2.TryGetComponent(out MyComponent1 rot);
        IsTrue(success);
        AreEqual(1, rot.a);
        
        success = player2.TryGetComponent(out Scale3 _);
        IsFalse(success);
        
        success = player2.TryGetComponent(out MyComponent2 _);
        IsFalse(success);
        //
        var start = Mem.GetAllocatedBytes();
        player2.TryGetComponent(out  pos);
        player2.TryGetComponent(out rot);
        player2.TryGetComponent(out Scale3 _);
        player2.TryGetComponent(out MyComponent2 _);
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_1_custom_Entity_Extensions() {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        player.AddComponent<MyComponent1>();
        player.AddComponent<MyComponent2>();
        
        player.MyComponent1().a = 1;
        AreEqual(1, player.MyComponent1().a);
        
        player.MyComponent2().b = 2;
        AreEqual(2, player.MyComponent2().b);
    }
    
    /// <summary>cover <see cref="EntityStoreBase.GetArchetypeWithout"/></summary>
    [Test]
    public static void Test_2_add_remove_struct_component() {
        var store  = new EntityStore(PidType.RandomPids);
        var player = store.CreateEntity();
        IsTrue(player.AddComponent(new MyComponent1()));
        IsTrue(player.AddComponent(new Position()));
        AreEqual(2,     player.Archetype.ComponentCount);

        // remove in same order to force creation of new Archetype based on exiting
        IsTrue(player.RemoveComponent<MyComponent1>());
        AreEqual(1,     player.Archetype.ComponentCount);
        
        // Archetype remains unchanged. component is already removed
        IsFalse(player.RemoveComponent<MyComponent1>());
        AreEqual(1,     player.Archetype.ComponentCount);
    }
    
    [Test]
    public static void Test_2_CreateEntity() {
        var store = new EntityStore(PidType.RandomPids);
        for (int n = 0; n < 512; n++) {
            var player1 =store.CreateEntity();
            player1.AddComponent<Position>();
        }
    }
    
    /// <summary>
    /// Note:
    /// Create dummy entities in same <see cref="Archetype"/> to avoid resize of
    /// <see cref="EntityStore.nodes"/> and <see cref="Archetype.entityIds"/> 
    /// </summary>
    [Test]
    public static void Test_3_AddPosition()
    {
        var store = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity();
        IsTrue (entity1.AddComponent(new Position { x = 1,  y = 2 }));
        IsFalse(entity1.AddComponent(new Position { x = 10, y = 11 }));
        
        // Create dummy entities. See Note above
        entity1.Archetype.CreateEntity();
        entity1.Archetype.CreateEntity();
        entity1.Archetype.CreateEntity();
        entity1.Archetype.CreateEntity();
        
        var start = Mem.GetAllocatedBytes();
        var entity2 = store.CreateEntity();
        entity2.AddComponent<Position>();
        entity2.AddComponent<Position>();

        Mem.AssertNoAlloc(start);
        AreEqual(10f, entity1.Position.x);
        AreEqual(11f, entity1.Position.y);
    }
    
    [Test]
    public static void Test_4_GetArchetype() {
        var store   = new EntityStore(PidType.RandomPids);
        AreEqual(1, store.Archetypes.Length);
        
        var type1   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        var type2   = store.GetArchetype(ComponentTypes.Get<Rotation, Position>());
        AreSame(type1, type2);
        
        type1   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, MyComponent1>());
        type2   = store.GetArchetype(ComponentTypes.Get<MyComponent1, Position, Rotation>());
        AreSame(type1, type2);
        
        type1       = store.GetArchetype(ComponentTypes.Get<Position, Rotation, MyComponent1, MyComponent2>());
        type2       = store.GetArchetype(ComponentTypes.Get<MyComponent1, Position, Rotation,  MyComponent2>());
        AreSame(type1, type2);
        
        type1       = store.GetArchetype(ComponentTypes.Get<Position, Rotation, MyComponent1, Scale3, MyComponent2>());
        type2       = store.GetArchetype(ComponentTypes.Get<Scale3, MyComponent1, Position, Rotation, MyComponent2>());
        AreSame(type1, type2);
        
        AreEqual(5, store.Archetypes.Length);
        AreEqual(0, type1.Count);
        AreEqual(0, type2.Count);
    }
    
    [Test]
    public static void Test_5_AddPositionRotation() {
        var store       = new EntityStore(PidType.RandomPids);
        
        var posType     = store.GetArchetype(ComponentTypes.Get<Position>());
        var posRotType  = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        AreEqual(1,         posType.ComponentTypes.Count);
        AreEqual(2,         posRotType.ComponentTypes.Count);
        
        var player1  = store.CreateEntity();
        AreEqual("[]  entities: 1", player1.Archetype.ToString());
        
        var position = new Position { x = 1 };
        player1.AddComponent(position);
        AreEqual(1f,        player1.Position.x);
        AreEqual(1,         posType.Count);
        AreEqual(1,         posType.EntityIds.Length);
        AreEqual("[Position]  entities: 1", posType.ToString());
        var components = player1.Archetype.Components<Position>();
        AreEqual(1,         components.Length);
        AreEqual(1,         components[0].x);
        
        
        player1.AddComponent<Rotation>(); // changes Archetype of player1
        AreEqual(0,         posType.Count);
        AreEqual(1f,        player1.Position.x);
        
        var player2 = store.CreateEntity();
        player2.AddComponent<Position>();
        AreEqual(1,         posType.Count);
        
        player2.AddComponent<Rotation>();   // changes Archetype of player2
        AreEqual(0,         posType.Count);
        AreEqual(2,         posRotType.Count);
        AreEqual(2,         store.Count);
        var entity1 = store.GetEntityById(1);
        IsTrue  (player1 == entity1);
        AreEqual(0,         entity1.Parent.Id);
        AreEqual("entities: 2", store.ToString());
    }
    
    [Test]
    public static void Test_HasPosition_Perf() {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        player.AddComponent(new Position());
        IsTrue(player.HasPosition);
    }
    
    [Test]
    public static void Test_GetComponent_Perf() {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        player.AddComponent(new MyComponent1());
        IsTrue(player.HasComponent<MyComponent1>());
    }

    [Test]
    public static void Test_6_AddRotation() {
        var store   = new EntityStore(PidType.RandomPids);
        var type    = store.GetArchetype(ComponentTypes.Get<Rotation, Scale3>());
        
        var player = store.CreateEntity();
        AreEqual(0,     player.Components.Count);
        
        var rotation = new Rotation { x = 1, y = 2 };
        player.AddComponent(rotation);
        var scale    = new Scale3   { x = 3, y = 4 };
        player.AddComponent(scale);
        AreEqual(1,     type.Count);
        AreEqual(1f,    player.Rotation.x);
        AreEqual(2f,    player.Rotation.y);
        AreEqual(3f,    player.Scale3.x);
        AreEqual(4f,    player.Scale3.y);
        
        var components  =       player.Components;
        AreEqual(2,             components.Count);

        int count = 0;
        foreach (var component in components) {
#pragma warning disable CS0618 // Type or member is obsolete
            switch (count++) {
                case 0:
                    AreEqual("Component: [Rotation]",   component.ToString());
                    AreEqual("Component: [Rotation]",   component.Type.ToString());
                    AreEqual("1, 2, 0, 0",              component.Value.ToString());    break;
                case 1:
                    AreEqual("Component: [Scale3]",     component.ToString());
                    AreEqual("Component: [Scale3]",     component.Type.ToString());
                    AreEqual("3, 4, 0",                 component.Value.ToString());    break;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
        AreEqual(2, count);
    }
    
    [Test]
    public static void Test_ComponentEnumerator()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        AreEqual(0,     player.Components.Count);
        
        var rotation = new Rotation { x = 1, y = 2 };
        player.AddComponent(rotation);
        var scale    = new Scale3   { x = 3, y = 4 };
        player.AddComponent(scale);
        AreEqual("Components: [Rotation, Scale3]",     player.Components.ToString());
        {
            IEnumerable<EntityComponent> components = player.Components;
            int count = 0;
            foreach (var _ in components) {
                count++;
            }
            AreEqual(2, count);
        } {
            IEnumerable components = player.Components;
            int count = 0;
            foreach (var _ in components) {
                count++;
            }
            AreEqual(2, count);
        } {
            ComponentEnumerator enumerator = player.Components.GetEnumerator();
            while (enumerator.MoveNext()) { }
            enumerator.Reset();
            
            int count = 0;
            while (enumerator.MoveNext()) {
                count++;
            }
            enumerator.Dispose();
            AreEqual(2, count);
        }
    }
    
    /// <summary>Test
    /// <see cref="Archetype.MoveEntityTo"/>
    /// <see cref="StructHeap{T}.MoveComponent"/>
    /// </summary>
    [Test]
    public static void Test_7_MoveComponent() {
        var store   = new EntityStore(PidType.RandomPids);
        AreEqual(1, store.Archetypes.Length);
        
        var player1 = store.CreateEntity();
        var position1 = new Position { x = 1 };
        player1.AddComponent(position1);
        
        var player2 = store.CreateEntity();
        var position2 = new Position { x = 2 };
        player2.AddComponent(position2);
        
        var rotation1 = new Rotation { x = 3 };
        player1.AddComponent(rotation1); // adding Rotation changes Archetype
        
        AreEqual(1f, player1.Position.x);
        AreEqual(2f, player2.Position.x);
    }

    [Test]
    public static void Test_8_ModifyComponent() {
        var store = new EntityStore(PidType.RandomPids);
        var player = store.CreateEntity();
        player.AddComponent<Position>();
        // set via GetComponent<>()
        ref var pos = ref player.GetComponent<Position>();
        pos.x = 1;
        // read via Property
        var p2 = player.Position;
        AreEqual(1, p2.x);
    }
    
    [Test]
    public static void Test_9_TestMissingComponent()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        Throws<NullReferenceException>(() => {
            player.GetComponent<MyInvalidComponent>();
        });
        
        // throws currently no exception
        player.RemoveComponent<MyInvalidComponent>();
    }
    
    /// <summary>Similar to <see cref="Raw.Test_RawEntities.Test_RawEntities_Components"/></summary>
    [Test]
    public static void Test_9_RemoveComponent() {
        var store   = new EntityStore(PidType.RandomPids);
        var type1 = store.GetArchetype(ComponentTypes.Get<Position>());
        var type2 = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        
        var entity1  = store.CreateEntity();
        entity1.AddComponent(new Position { x = 1 });
        AreEqual(1,     type1.Count);
        AreEqual(1,     entity1.Archetype.ComponentCount);
        
        entity1.RemoveComponent<Position>();
        AreEqual(0,     type1.Count);
        AreEqual(0,     entity1.Archetype.ComponentCount);
        
        entity1.AddComponent(new Position { x = 1 });
        AreEqual(1,     type1.Count);
        AreEqual(1,     entity1.Archetype.ComponentCount);
        
        entity1.AddComponent(new Rotation { x = 2 });
        AreEqual(0,     type1.Count);
        AreEqual(1,     type2.Count);
        AreEqual(2,     entity1.Archetype.ComponentCount);
        
        entity1.RemoveComponent<Rotation>();
        AreEqual(1,     type1.Count);
        AreEqual(0,     type2.Count);
        AreEqual(1f,    entity1.Position.x);
        AreEqual(1,     entity1.Archetype.ComponentCount);
        //
        var entity2  = store.CreateEntity();
        entity2.AddComponent(new Position { x = 1 });   // possible alloc: resize type1.entityIds
        entity2.RemoveComponent<Position>();            // note: remove the last id in type1.entityIds => only type1.entityCount--  
        AreEqual(1,     type1.Count);
        AreEqual(0,     entity2.Archetype.ComponentCount);
        
        var start = Mem.GetAllocatedBytes();
        entity2.AddComponent(new Position { x = 1 });
        entity2.RemoveComponent<Position>();
        Mem.AssertNoAlloc(start);
        
        AreEqual(1,     type1.Count);
        AreEqual(0,     entity2.Archetype.ComponentCount);
    }
    
    [Test]
    public static void Test_9_Add_Remove_Component_Perf() {
        var store   = new EntityStore(PidType.RandomPids);
        var posType = store.GetArchetype(ComponentTypes.Get<Position>());
        store.CreateEntity().AddComponent<Position>();
        store.CreateEntity().AddComponent<Position>();
        store.CreateEntity().AddComponent<Position>();
        
        var entity  = store.CreateEntity();
        entity.AddComponent<Position>();    // force resize type1.entityIds
        
        var start = Mem.GetAllocatedBytes();
        int count = 10; // 100_000_000 ~ #PC: 5.624 ms
        for (var n = 0; n < count; n++) {
            entity.AddComponent<Position>();
            entity.RemoveComponent<Position>();
        }
        Mem.AssertNoAlloc(start);
        AreEqual(3, posType.Count);
    }
    
    [Test]
    public static void Test_9_Set_Name() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        IsFalse(entity.HasName);
        IsFalse(entity.HasPosition);
        IsFalse(entity.HasRotation);
        IsFalse(entity.HasScale3);
        IsFalse(entity.HasComponent<EntityName>());
        AreEqual("id: 1  []",           entity.ToString());
        
        entity.AddComponent(new EntityName("Hello"));
        AreEqual("'Hello'", entity.GetComponent<EntityName>().ToString());
        IsTrue(entity.HasName);
        IsTrue(entity.HasComponent<EntityName>());
        AreEqual("id: 1  \"Hello\"  [EntityName]",    entity.ToString());
        
        AreEqual("Hello",               entity.Name.value);
        AreEqual("Hello",               Encoding.UTF8.GetString(entity.Name.Utf8));
        
        entity.Name.value = null;
        AreEqual("id: 1  [EntityName]", entity.ToString());
        IsNull(                         entity.Name.Utf8);
    }
    
    [Test]
    public static void Test_StructComponent_EntityStore_creation_Perf() {
        _ = new EntityStore(PidType.RandomPids);
        var stopwatch =  new Stopwatch();
        stopwatch.Start();
        int count = 10; // 1_000_000 ~ #PC: 422 ms
        for (int n = 0; n < count; n++) {
            _ = new EntityStore(PidType.RandomPids);
        }
        Console.WriteLine($"EntityStore count: {count}, duration: {stopwatch.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_StructComponent_create_delete_entity() {
        var store   = new EntityStore(PidType.RandomPids);
        var arch0   = store.GetArchetype(default);
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        
        AreEqual(2, store.Count);
        AreEqual(2, store.Entities.Count);
        AreEqual(2, arch0.Count);
        AreEqual(2, arch0.Entities.Count);
        
        entity1.DeleteEntity();
        entity2.DeleteEntity();
        
        AreEqual(0, store.Count);
        AreEqual(0, store.Entities.Count);
        AreEqual(0, arch0.Count);
        AreEqual(0, arch0.Entities.Count);
    }
    
    [Test]
    public static void Test_StructComponent_add_remove_component() {
        var store   = new EntityStore(PidType.RandomPids);
        var arch0   = store.GetArchetype(default);
        var arch1   = store.GetArchetype(ComponentTypes.Get<Position>());
        var entity1 = store.CreateEntity();
        entity1.AddComponent<Position>();
        
        AreEqual(1, store.Count);
        AreEqual(1, store.Entities.Count);
        AreEqual(0, arch0.Count);
        AreEqual(0, arch0.Entities.Count);
        AreEqual(1, arch1.Count);
        AreEqual(1, arch1.Entities.Count);
        
        entity1.RemoveComponent<Position>();
        AreEqual(1, store.Count);
        AreEqual(1, store.Entities.Count);
        AreEqual(1, arch0.Count);
        AreEqual(1, arch0.Entities.Count);
        AreEqual(0, arch1.Count);
        AreEqual(0, arch1.Entities.Count);
    }
        
    [Test]
    public static void Test_StructComponent_Archetype_CreateEntity_default_components()
    {
        var store = new EntityStore(PidType.RandomPids);
                    store.GetArchetype(ComponentTypes.Get<Position>());
        var arch2 = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        var arch3 = store.GetArchetype(ComponentTypes.Get<Position, Rotation, MyComponent1>());
        
        arch2.CreateEntity();
        arch3.CreateEntity();
        var entity = arch3.CreateEntity();
        int count = 0;
        foreach (var _ in store.Entities) {
            count++;
        }
        AreEqual(3, count);
        AreEqual(3, store.Entities.Count);
        
        entity.Position.x = 123;
        entity.Rotation.x = 123;
        entity.MyComponent1().a = 123;
        entity.DeleteEntity();
        
        // ensure components of new entity have default values
        entity = arch3.CreateEntity();
        AreEqual(0, entity.Position.x);
        AreEqual(0, entity.Rotation.x);
        AreEqual(0, entity.MyComponent1().a);
    }
    
    [Test]
    public static void Test_StructComponent_Cache()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        
        IsTrue  (entity.AddComponent(new MyComponent1()));
        IsTrue  (entity.RemoveComponent<MyComponent1>());
        IsTrue  (entity.AddComponent(new MyComponent1 { a = 10 }));
        AreEqual(10, entity.GetComponent<MyComponent1>().a);
        IsTrue  (entity.RemoveComponent<MyComponent1>());
        
        IsTrue  (entity.AddComponent(new MyComponent2()));
        IsTrue  (entity.RemoveComponent<MyComponent2>());
        IsTrue  (entity.AddComponent(new MyComponent2 { b = 11 }));
        AreEqual(11, entity.GetComponent<MyComponent2>().b);
        IsTrue  (entity.RemoveComponent<MyComponent2>());
        
        IsTrue  (entity.AddComponent(new MyComponent3()));
        IsTrue  (entity.RemoveComponent<MyComponent3>());
        IsTrue  (entity.AddComponent(new MyComponent3 { b = 12 }));
        AreEqual(12, entity.GetComponent<MyComponent3>().b);
        IsTrue  (entity.RemoveComponent<MyComponent3>());
        
        IsTrue  (entity.AddComponent(new MyComponent4()));
        IsTrue  (entity.RemoveComponent<MyComponent4>());
        IsTrue  (entity.AddComponent(new MyComponent4 { b = 13 }));
        AreEqual(13, entity.GetComponent<MyComponent4>().b);
        IsTrue  (entity.RemoveComponent<MyComponent4>());
        
        IsTrue  (entity.AddComponent(new MyComponent5()));
        IsTrue  (entity.RemoveComponent<MyComponent5>());
        IsTrue  (entity.AddComponent(new MyComponent5 { b = 14 }));
        AreEqual(14, entity.GetComponent<MyComponent5>().b);
        IsTrue  (entity.RemoveComponent<MyComponent5>());
    }
    
}

}