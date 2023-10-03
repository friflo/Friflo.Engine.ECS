using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_ComponentReader
{
    private static readonly JsonValue json =
        new JsonValue("{ \"pos\": { \"x\": 1, \"y\": 2, \"z\": 3 }, \"scl3\": { \"x\": 4, \"y\": 5, \"z\": 6 } }");
    
    [Test]
    public static void Test_ReadStructComponents()
    {
        var store       = new EntityStore(100, PidType.UsePidAsId);
        store.RegisterStructComponent<Position>();
        store.RegisterStructComponent<Scale3>();
        
        var rootNode    = new DataNode { pid = 10, components = json, children = new List<int> { 11 } };
        var childNode   = new DataNode { pid = 11 };
        
        var root        = store.CreateFromDataNode(rootNode);
        var child       = store.CreateFromDataNode(childNode);
        AssertRootEntity(root);
        var type = store.GetArchetype<Position, Scale3>();
        AreEqual(1,     type.EntityCount);
        
        root.Position   = default;
        root.Scale3     = default;
        root            = store.CreateFromDataNode(rootNode);
        AssertRootEntity(root);
        AreEqual(1,     type.EntityCount);

        AreEqual(11,    child.Id);
    }
    
    private static void AssertRootEntity(GameEntity root) {
        AreEqual(1,     root.ChildCount);
        AreEqual(11,    root.ChildNodes.Ids[0]);
        AreEqual(2,     root.ComponentCount);
        AreEqual(1f,    root.Position.x);
        AreEqual(2f,    root.Position.y);
        AreEqual(3f,    root.Position.z);
        AreEqual(4f,    root.Scale3.x);
        AreEqual(5f,    root.Scale3.y);
        AreEqual(6f,    root.Scale3.z);
    }
    
    [Test]
    public static void Test_ReadStructComponents_Perf()
    {
        var store       = new EntityStore(100, PidType.UsePidAsId);
        store.RegisterStructComponent<Position>();
        store.RegisterStructComponent<Scale3>();
        
        var rootNode    = new DataNode { pid = 10, components = json, children = new List<int> { 11 } };
        
        const int count = 10; // todo 
        for (int n = 0; n < count; n++)
        {
            var root = store.CreateFromDataNode(rootNode);
            root.DeleteEntity();
        }
    }
}

