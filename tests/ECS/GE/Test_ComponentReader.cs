using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;



public static class Test_ComponentReader
{

    
    private static readonly JsonValue rootComponents =
        new JsonValue("{ \"pos\": { \"x\": 1, \"y\": 1, \"z\": 1 }, \"scl3\": { \"x\": 2, \"y\": 2, \"z\": 2 } }");
    
    private static readonly JsonValue childComponents =
        new JsonValue("{ \"pos\": { \"x\": 3, \"y\": 3, \"z\": 3 }, \"scl3\": { \"x\": 4, \"y\": 4, \"z\": 4 } }");
    
    [Test]
    public static void Test_ComponentReader_read_struct_components()
    {
        var store       = TestUtils.CreateGameEntityStore(out var database);
        
        var rootNode    = new DataNode { pid = 10, components = rootComponents, children = new List<long> { 11 } };
        var childNode   = new DataNode { pid = 11, components = childComponents };
        
        var root        = database.LoadEntity(rootNode, out _);
        var child       = database.LoadEntity(childNode, out _);
        AssertRootEntity(root);
        AssertChildEntity(child);
        var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read root DataNode again
        root.Position   = default;
        root.Scale3     = default;
        root            = database.LoadEntity(rootNode, out _);
        AssertRootEntity(root);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read child DataNode again
        child.Position  = default;
        child.Scale3    = default;
        child           = database.LoadEntity(childNode, out _);
        AssertChildEntity(child);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
    }
    
    /// <summary>test structure change in <see cref="ComponentReader.SetEntityArchetype"/></summary>
    [Test]
    public static void Test_ComponentReader_change_archetype()
    {
        var store       = TestUtils.CreateGameEntityStore(out var database);
        var root        = store.CreateEntity(10);
        root.AddComponent(new Scale3(1, 2, 3));
        IsTrue  (root.HasScale3);
        IsFalse (root.HasPosition);
        
        var rootNode    = new DataNode { pid = 10, components = rootComponents };
        var rootResult  = database.LoadEntity(rootNode, out _);  // archetype changes
        AreSame (root, rootResult);
        IsTrue  (root.HasScale3);   // could change behavior and remove all components not present in DataNode components
        IsTrue  (root.HasPosition);
    }
    
    /// <summary>test structure change in <see cref="ComponentReader.SetEntityArchetype"/></summary>
    [Test]
    public static void Test_ComponentReader_read_components_null()
    {
        TestUtils.CreateGameEntityStore(out var database);
        var node    = new DataNode { pid = 10, components = default };
        var entity  = database.LoadEntity(node, out var error);
        AreEqual(0, entity.ComponentCount);
        IsNull  (error);
    }
    
    /// <summary>test structure change in <see cref="ComponentReader.SetEntityArchetype"/></summary>
    [Test]
    public static void Test_ComponentReader_read_components_empty()
    {
        TestUtils.CreateGameEntityStore(out var database);
        var node    = new DataNode { pid = 10, components = new JsonValue("{}") };
        var entity  = database.LoadEntity(node, out var error);
        AreEqual(0, entity.ComponentCount);
        IsNull  (error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_tags()
    {
        TestUtils.CreateGameEntityStore(out var database);
        var node    = new DataNode { pid = 10, tags = new List<string> { nameof(TestTag) } };
        var entity  = database.LoadEntity(node, out _);
        AreEqual(0, entity.ComponentCount);
        IsTrue  (entity.Tags.Has<TestTag>());
    }
    
    [Test]
    public static void Test_ComponentReader_read_invalid_component()
    {
        TestUtils.CreateGameEntityStore(out var database);
        var json    = new JsonValue("{ \"pos\": [] }");
        var node    = new DataNode { pid = 10, components = json };
        var entity  = database.LoadEntity(node, out var error);
        NotNull(entity);
        AreEqual("component must be an object. was ArrayStart. id: 10, component: 'pos'", error);
    }
    
    [Test]
    public static void Test_ComponentReader_read_invalid_components()
    {
        TestUtils.CreateGameEntityStore(out var database);
        var node    = new DataNode { pid = 10, components = new JsonValue("123") };
        var entity  = database.LoadEntity(node, out var error);
        NotNull(entity);
        AreEqual("expect 'components' == object or null. id: 10. was: ValueNumber", error);
        
        node        = new DataNode { pid = 10, components = new JsonValue("invalid") };
        entity      = database.LoadEntity(node, out error);
        NotNull(entity);
        AreEqual("unexpected character while reading value. Found: i path: '(root)' at position: 1. id: 10", error);
    }
    
    /// <summary>cover <see cref="ComponentReader.Read"/></summary>
    [Test]
    public static void Test_ComponentReader_DataNode_assertions()
    {
        {
            TestUtils.CreateGameEntityStore(out var database);
            var e = Throws<ArgumentNullException>(() => {
                database.LoadEntity(null, out _);
            });
            AreEqual("Value cannot be null. (Parameter 'dataNode')", e!.Message);
        } {
            TestUtils.CreateGameEntityStore(out var database);
            var childNode   = new DataNode { pid = int.MaxValue + 1L };
            var e = Throws<ArgumentException>(() => {
                database.LoadEntity(childNode, out _);
            });
            AreEqual("pid mus be in range [0, 2147483647]. was: {pid} (Parameter 'dataNode')", e!.Message);
        }
    }
    
    private static void AssertRootEntity(GameEntity root) {
        AreEqual(10,    root.Id);
        AreEqual(1,     root.ChildCount);
        AreEqual(11,    root.ChildNodes.Ids[0]);
        AreEqual(2,     root.ComponentCount);
        AreEqual(1f,    root.Position.x);
        AreEqual(1f,    root.Position.y);
        AreEqual(1f,    root.Position.z);
        AreEqual(2f,    root.Scale3.x);
        AreEqual(2f,    root.Scale3.y);
        AreEqual(2f,    root.Scale3.z);
    }
    
    private static void AssertChildEntity(GameEntity child) {
        AreEqual(11,    child.Id);
        AreEqual(0,     child.ChildCount);
        AreEqual(2,     child.ComponentCount);
        AreEqual(3f,    child.Position.x);
        AreEqual(3f,    child.Position.y);
        AreEqual(3f,    child.Position.z);
        AreEqual(4f,    child.Scale3.x);
        AreEqual(4f,    child.Scale3.y);
        AreEqual(4f,    child.Scale3.z);
    }
    
    [NUnit.Framework.IgnoreAttribute("remove childIds reallocation")][Test]
    public static void Test_ComponentReader_read_struct_components_Mem()
    {
        var store       = TestUtils.CreateGameEntityStore(out var database);
        
        var rootNode    = new DataNode { pid = 10, components = rootComponents, children = new List<long> { 11 } };
        var childNode   = new DataNode { pid = 11, components = childComponents };
        
        var root        = database.LoadEntity(rootNode, out _);
        var child       = database.LoadEntity(childNode, out _);
        AssertRootEntity(root);
        AssertChildEntity(child);
        var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read same DataNode again
        root.Position   = default;
        root.Scale3     = default;
        var start       = Mem.GetAllocatedBytes();
        root            = database.LoadEntity(rootNode, out _);
        Mem.AssertNoAlloc(start);
        AssertRootEntity(root);
        AssertChildEntity(child);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
    }
    
    [Test]
    public static void Test_ComponentReader_read_struct_components_Perf()
    {
        TestUtils.CreateGameEntityStore(out var database);
        
        var rootNode    = new DataNode { pid = 10, components = rootComponents, children = new List<long> { 11 } };
        
        const int count = 10; // 1_000_000 ~ 2.639 ms (bottleneck parsing JSON to structs)
        for (int n = 0; n < count; n++)
        {
            var root = database.LoadEntity(rootNode, out _);
            root.DeleteEntity();
        }
    }
    
    private static readonly JsonValue classComponents = new JsonValue("{ \"testRef1\": { \"val1\": 2 } }");
    
    [Test]
    public static void Test_ComponentReader_read_class_components()
    {
        TestUtils.CreateGameEntityStore(out var database);
        
        var rootNode    = new DataNode { pid = 10, components = classComponents, children = new List<long> { 11 } };

        var root        = database.LoadEntity(rootNode, out _);
        AreEqual(1,     root.ClassComponents.Length);
        var comp1       = root.GetClassComponent<TestRefComponent1>();
        AreEqual(2,     comp1.val1);
        comp1.val1      = -1;
        
        // --- read same DataNode again
        database.LoadEntity(rootNode, out _);
        var comp2       = root.GetClassComponent<TestRefComponent1>();
        AreEqual(2,     comp2.val1);
        AreSame(comp1, comp2);
    }
    
    [Test]
    public static void Test_ComponentReader_read_class_components_Perf()
    {
        TestUtils.CreateGameEntityStore(out var database);
        
        var rootNode    = new DataNode { pid = 10, components = classComponents, children = new List<long> { 11 } };

        const int count = 10; // 5_000_000 ~ 8.090 ms   todo check degradation from 3.528 ms
        for (int n = 0; n < count; n++) {
            database.LoadEntity(rootNode, out _);
        }
    }
}

