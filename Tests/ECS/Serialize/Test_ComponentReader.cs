using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

namespace Tests.ECS.Serialize;

// ReSharper disable once InconsistentNaming
public static class Test_ComponentReader
{
    /// <summary>
    /// Need to create <see cref="JsonValue"/> instances. They can be modified in <see cref="Friflo.Json.Fliox.JsonValue.Copy"/>
    /// </summary>
    internal static JsonValue RootComponents =>
        new JsonValue("{ \"pos\": { \"x\": 1, \"y\": 1, \"z\": 1 }, \"scl3\": { \"x\": 2, \"y\": 2, \"z\": 2 } }");
    
    internal static JsonValue ChildComponents =>
        new JsonValue("{ \"pos\": { \"x\": 3, \"y\": 3, \"z\": 3 }, \"scl3\": { \"x\": 4, \"y\": 4, \"z\": 4 } }");
    
    [Test]
    public static void Test_ComponentReader_read_components()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var rootNode    = new DataEntity { pid = 10, components = RootComponents, children = new List<long> { 11 } };
        var childNode   = new DataEntity { pid = 11, components = ChildComponents };
        
        var root        = converter.DataEntityToEntity(rootNode, store, out _);
        var child       = converter.DataEntityToEntity(childNode, store, out _);
        AssertRootEntity(root, 2);
        AssertChildEntity(child);
        var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read root DataEntity again
        root.Position   = default;
        root.Scale3     = default;
        root            = converter.DataEntityToEntity(rootNode, store, out _);
        AssertRootEntity(root, 2);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read child DataEntity again
        child.Position  = default;
        child.Scale3    = default;
        child           = converter.DataEntityToEntity(childNode, store, out _);
        AssertChildEntity(child);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
    }
    
    /// <summary>test structure change in <see cref="ComponentReader.SetEntityArchetype"/></summary>
    [Test]
    public static void Test_ComponentReader_change_archetype()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var root        = store.CreateEntity(10);
        root.AddComponent(new Scale3(1, 2, 3));
        IsTrue  (root.HasScale3);
        IsFalse (root.HasPosition);
        
        var rootNode    = new DataEntity { pid = 10, components = RootComponents };
        var rootResult  = converter.DataEntityToEntity(rootNode, store, out _);  // archetype changes
        IsTrue  (root == rootResult);
        IsTrue  (root.HasScale3);   // could change script and remove all components not present in DataEntity components
        IsTrue  (root.HasPosition);
    }
    
    /// <summary>test structure change in <see cref="ComponentReader.SetEntityArchetype"/></summary>
    [Test]
    public static void Test_ComponentReader_read_components_null()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;

        var node    = new DataEntity { pid = 10, components = default };
        var entity  = converter.DataEntityToEntity(node, store, out var error);
        AreEqual(0, entity.Scripts.Length + entity.Archetype.ComponentCount);
        IsNull  (error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_children()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        ComponentReader_read_children(store);
    }
    
    [Test]
    public static void Test_ComponentReader_read_children_events()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var events  = Events.SetHandlerSeq(store, (args, seq) => {
            switch (seq) {
                // --- initial children: [2, 3, 4, 5]
                case 0:     AreEqual("entity: 1 - Add ChildIds[0] = 2",     args.ToString());   return;
                case 1:     AreEqual("entity: 1 - Add ChildIds[1] = 3",     args.ToString());   return;
                case 2:     AreEqual("entity: 1 - Add ChildIds[2] = 4",     args.ToString());   return;
                case 3:     AreEqual("entity: 1 - Add ChildIds[3] = 5",     args.ToString());   return;
                // --- changed children: [6, 4, 2, 5]
                case 4:     AreEqual("entity: 1 - Remove ChildIds[1] = 3",  args.ToString());   return;
                case 5:     AreEqual("entity: 1 - Add ChildIds[0] = 6",     args.ToString());   return;
                case 6:     AreEqual("entity: 1 - Remove ChildIds[2] = 4",  args.ToString());   return;
                case 7:     AreEqual("entity: 1 - Remove ChildIds[1] = 2",  args.ToString());   return;
                case 8:     AreEqual("entity: 1 - Add ChildIds[1] = 4",     args.ToString());   return;
                case 9:     AreEqual("entity: 1 - Add ChildIds[2] = 2",     args.ToString());   return;
                default:    Fail($"unexpected seq: {seq}");                                     return;
            }
        });
        ComponentReader_read_children(store);
        AreEqual(10, events.seq);
    }

    private static void ComponentReader_read_children(EntityStore store)
    {
        var converter   = EntityConverter.Default;

        var dataRoot    = new DataEntity { pid = 1, children = new List<long> { 2, 3, 4, 5 } };
        var root        = converter.DataEntityToEntity(dataRoot, store, out _);
        
        AreEqual(new [] { 2, 3, 4, 5 }, root.ChildIds.ToArray());
        for (int n = 2; n <= 6; n++) {
            converter.DataEntityToEntity(new DataEntity { pid = n }, store, out _);
        }
        AreEqual(6,     store.EntityCount);
        
        dataRoot    = new DataEntity { pid = 1, children = new List<long> { 6, 4, 2, 5 } };
        root        = converter.DataEntityToEntity(dataRoot, store, out _);
        
        AreEqual(new [] { 6, 4, 2, 5 }, root.ChildIds.ToArray());
        AreEqual(6,     store.EntityCount);
    }
    
    /// <summary>test structure change in <see cref="ComponentReader.SetEntityArchetype"/></summary>
    [Test]
    public static void Test_ComponentReader_read_components_empty()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var node    = new DataEntity { pid = 10, components = new JsonValue("{}") };
        var entity  = converter.DataEntityToEntity(node, store, out var error);
        AreEqual(0, entity.Scripts.Length + entity.Archetype.ComponentCount);
        IsNull  (error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_EntityName()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var node    = new DataEntity { pid = 10, components = new JsonValue("{\"name\":{\"value\":\"test\"}}") };
        var entity  = converter.DataEntityToEntity(node, store, out var error);
        
        AreEqual("test", entity.GetComponent<EntityName>().value);
        IsNull  (error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_tags()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var node    = new DataEntity { pid = 10, tags = new List<string> { "test-tag", nameof(TestTag3) } };
        var entity  = converter.DataEntityToEntity(node, store, out _);
        AreEqual(0, entity.Scripts.Length + entity.Archetype.ComponentCount);
        IsTrue  (entity.Tags.Has<TestTag>());
    }
    
    [Test]
    public static void Test_ComponentReader_read_invalid_component()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var json    = new JsonValue("{ \"pos\": [] }");
        var node    = new DataEntity { pid = 10, components = json };
        var entity  = converter.DataEntityToEntity(node, store, out var error);
        NotNull(entity);
        AreEqual("'components' element must be an object. was ArrayStart. id: 10, component: 'pos'", error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_invalid_components()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var node    = new DataEntity { pid = 10, components = new JsonValue("123") };
        var entity  = converter.DataEntityToEntity(node, store, out var error);
        NotNull(entity);
        AreEqual("expect 'components' == object or null. id: 10. was: ValueNumber", error);
        
        node        = new DataEntity { pid = 10, components = new JsonValue("invalid") };
        entity      = converter.DataEntityToEntity(node, store, out error);
        NotNull(entity);
        AreEqual("unexpected character while reading value. Found: i path: '(root)' at position: 1. id: 10", error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_component_error()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var node    = new DataEntity { pid = 10, components = new JsonValue("{\"pos\":{\"x\":[]}}") };
        var entity  = converter.DataEntityToEntity(node, store, out var error);
        AreEqual("'components[pos]' - Cannot assign array to float. got: [...] path: 'x[]' at position: 6", error);
        NotNull(entity);
    }
    
    /// <summary>cover <see cref="ComponentReader.Read"/></summary>
    [Test]
    public static void Test_ComponentReader_DataEntity_assertions()
    {
        {
            var store       = new EntityStore(PidType.UsePidAsId);
            var converter   = EntityConverter.Default;
        
            var e = Throws<ArgumentNullException>(() => {
                converter.DataEntityToEntity(null, store, out _);
            });
            AreEqual("Value cannot be null. (Parameter 'dataEntity')", e!.Message);
        } {
            var store       = new EntityStore(PidType.UsePidAsId);
            var converter   = EntityConverter.Default;
        
            var childNode   = new DataEntity { pid = int.MaxValue + 1L };
            var e = Throws<ArgumentException>(() => {
                converter.DataEntityToEntity(childNode, store, out _);
            });
            AreEqual("pid must be in range [1, 2147483647] when using PidType.UsePidAsId. was: 2147483648 (Parameter 'DataEntity.pid')", e!.Message);
        }
    }
    
    internal static void AssertRootEntity(Entity root, int componentCount) {
        AreEqual(10,                root.Id);
        AreEqual(1,                 root.ChildCount);
        AreEqual(11,                root.ChildEntities.Ids[0]);
        AreEqual(componentCount,    root.Archetype.ComponentCount);
        if (componentCount == 0) {
            return;
        } 
        AreEqual(1f,                root.Position.x);
        AreEqual(1f,                root.Position.y);
        AreEqual(1f,                root.Position.z);
        AreEqual(2f,                root.Scale3.x);
        AreEqual(2f,                root.Scale3.y);
        AreEqual(2f,                root.Scale3.z);
    }
    
    internal static void AssertChildEntity(Entity child) {
        AreEqual(11,    child.Id);
        AreEqual(0,     child.ChildCount);
        AreEqual(2,     child.Archetype.ComponentCount);
        AreEqual(3f,    child.Position.x);
        AreEqual(3f,    child.Position.y);
        AreEqual(3f,    child.Position.z);
        AreEqual(4f,    child.Scale3.x);
        AreEqual(4f,    child.Scale3.y);
        AreEqual(4f,    child.Scale3.z);
    }
    
    /// <remarks>
    /// Fliox deserializer allocates memory for component structs => no components are added in test
    /// </remarks>
    [Test]
    public static void Test_ComponentReader_DataEntityToEntity_Mem()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var rootData    = new DataEntity { pid = 10, children = new List<long> { 11 } };
        var childData   = new DataEntity { pid = 11, components = ChildComponents };
        
        var root        = converter.DataEntityToEntity(rootData,  store, out _);
        var child       = converter.DataEntityToEntity(childData, store, out _);
        AssertRootEntity(root, 0);  // 0 -> Fliox deserializer allocates memory for component structs
        AssertChildEntity(child);
        // var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        // AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read same DataEntity again
        // root.Position   = default;
        // root.Scale3     = default;
        var start       = Mem.GetAllocatedBytes();
        root            = converter.DataEntityToEntity(rootData, store, out _);
        Mem.AssertNoAlloc(start);
        AssertRootEntity(root, 0);// 0 -> Fliox deserializer allocates memory for component structs
        AssertChildEntity(child);
        // AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
    }
    
    [Test]
    public static void Test_ComponentReader_read_components_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var rootNode    = new DataEntity { pid = 10, components = RootComponents, children = new List<long> { 11 } };
        
        const int count = 10; // 1_000_000 ~ 2.639 ms (bottleneck parsing JSON to structs)
        for (int n = 0; n < count; n++)
        {
            var root = converter.DataEntityToEntity(rootNode, store, out _);
            root.DeleteEntity();
        }
    }
    
    private static JsonValue Script => new JsonValue("{ \"script1\": { \"val1\": 2 } }");
    
    /// <summary> Cover also remove script in <see cref="ComponentReader.ReadComponents"/> </summary>
    [Test]
    public static void Test_ComponentReader_read_script()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var rootNode    = new DataEntity { pid = 10, components = Script, children = new List<long> { 11 } };

        var root        = converter.DataEntityToEntity(rootNode, store, out _);
        AreEqual(1,     root.Scripts.Length);
        var script1     = root.GetScript<TestScript1>();
        AreEqual(2,     script1.val1);
        script1.val1      = -1;
        
        // --- read same DataEntity again
        converter.DataEntityToEntity(rootNode, store, out _);
        var comp2       = root.GetScript<TestScript1>();
        AreEqual(2,     comp2.val1);
        AreSame(script1,comp2);
        
        // --- remove script from JSON components read again
        rootNode.components = new JsonValue("{ }");
        converter.DataEntityToEntity(rootNode, store, out _);
        var comp3       = root.GetScript<TestScript1>();
        IsNull(comp3);
    }
    
    [Test]
    public static void Test_ComponentReader_read_script_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var rootNode    = new DataEntity { pid = 10, components = Script, children = new List<long> { 11 } };

        const int count = 10; // 5_000_000 ~ 8.090 ms   todo check degradation from 3.528 ms
        for (int n = 0; n < count; n++) {
            converter.DataEntityToEntity(rootNode, store, out _);
        }
    }
    
    private static JsonValue Scripts => new JsonValue(
        "{ \"script1\": { \"val1\": 11 }, \"script2\": { \"val2\": 22 }, \"script3\": { \"val3\": 33 } }");
    
    /// <summary>Cover <see cref="EntityStore.AppendScript"/></summary>
    [Test]
    public static void Test_ComponentReader_read_multiple_scripts()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var rootNode    = new DataEntity { pid = 10, components = Scripts };

        var root        = converter.DataEntityToEntity(rootNode, store, out _);
        AreEqual(3,     root.Scripts.Length);
        var script1   = root.GetScript<TestScript1>();
        AreEqual(11,    script1.val1);
        var script2   = root.GetScript<TestScript2>();
        AreEqual(22,    script2.val2);
        var script3   = root.GetScript<TestScript3>();
        AreEqual(33,    script3.val3);
        
        script1.val1      = -1;
        script2.val2      = -1;
        script3.val3      = -1;
        
        // --- read same DataEntity again
        converter.DataEntityToEntity(rootNode, store, out _);
        AreEqual(3,     root.Scripts.Length);
        script1       = root.GetScript<TestScript1>();
        AreEqual(11,    script1.val1);
        script2       = root.GetScript<TestScript2>();
        AreEqual(22,    script2.val2);
        script3    = root.GetScript<TestScript3>();
        AreEqual(33,    script3.val3);
    }
    
    [Test]
    public static void Test_ComponentReader_Load_DataEntity_UsePidAsId()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var children    = new List<long>();
        for (int n = 0; n < 100; n++) {
            children.Add(n + 20);
        }
        var node    = new DataEntity{ pid = 10, children = children };
        var entity  = converter.DataEntityToEntity(node, store, out _);
        
        AreEqual(10,    store.PidToId(10L));
        AreEqual(10,    store.GetEntityByPid(10L).Pid);
        AreEqual(10,    entity.Id);
        AreEqual(100,   entity.ChildEntities.Count);
        var node10 = store.GetEntityNode(10);
        AreEqual(100,   node10.ChildCount);
        AreEqual(10,    node10.Pid);
        var node20 = store.GetEntityNode(20);
        AreEqual(10,    node20.ParentId);
        AreEqual(20,    node20.Pid);
        var childIds = node10.ChildIds;
        for (int n = 0; n < 100; n++) {
            AreEqual(n + 20, childIds[n]);
        }
        AreEqual(1,     store.EntityCount);
    }
    
    [Test]
    public static void Test_ComponentReader_Load_DataEntity_RandomPids() {
        var store       = new EntityStore();
        var converter   = EntityConverter.Default;
        
        var children    = new List<long>();
        for (int n = 0; n < 100; n++) {
            children.Add(n + 20);
        }
        var node        = new DataEntity{ pid = 10, children = children };
        var entity      = converter.DataEntityToEntity(node, store, out _);
        
        AreEqual(1,     store.PidToId(10L));
        AreEqual(1,     store.GetEntityByPid(10L).Id);
        AreEqual(1,     entity.Id);
        AreEqual(100,   entity.ChildEntities.Count);
        var node1 = store.GetEntityNode(1);
        AreEqual(100,   node1.ChildCount);
        AreEqual(10,    node1.Pid);
        var node2 = store.GetEntityNode(2);
        AreEqual(1,     node2.ParentId);
        AreEqual(20,    node2.Pid);
        var childIds = node1.ChildIds;
        for (int n = 0; n < 100; n++) {
            AreEqual(n + 2, childIds[n]);
        }
        AreEqual(1,     store.EntityCount);
    }
    
    [Test]
    public static void Test_ComponentReader_assertions() {
        var store       = new EntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        {
            var e = Throws<ArgumentNullException>(() => {
                converter.DataEntityToEntity(null, store, out _);    
            });
            AreEqual("Value cannot be null. (Parameter 'dataEntity')", e!.Message);
        } {
            var e = Throws<ArgumentNullException>(() => {
                converter.EntityToDataEntity(default, null, false);    
            });
            AreEqual("Value cannot be null. (Parameter 'entity')", e!.Message);
        }
    }
}

