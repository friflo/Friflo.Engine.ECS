using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox.Hub.Host;
using NUnit.Framework;
using Tests.ECS;
using Tests.ECS.GE;
using static NUnit.Framework.Assert;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.Client;

public static class Test_Client
{
    private static GameDatabase CreateClientDatabase(GameEntityStore store) {
        var hub     = new FlioxHub(new MemoryDatabase("test"));
        var client  = new GameClient(hub);
        return new GameDatabase(store, client);
    }
    
    [Test]
    public static void Test_Client_read_components()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var database    = CreateClientDatabase(store);
        
        var rootNode    = new DatabaseEntity { pid = 10L, components = Test_ComponentReader.rootComponents, children = new List<long> { 11 } };
        var childNode   = new DatabaseEntity { pid = 11L, components = Test_ComponentReader.childComponents };
        database.Entities.Add(rootNode);
        database.Entities.Add(childNode);
        int n = 0; 
        foreach (var entity in database.Entities) {
            n++;
            NotNull(entity);
        }
        AreEqual(2, n);
        
        var root        = database.LoadGameEntity(10L, out _);
        var child       = database.LoadGameEntity(11L, out _);
        Test_ComponentReader.AssertRootEntity(root);
        Test_ComponentReader.AssertChildEntity(child);
        var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read root DatabaseEntity again
        root.Position   = default;
        root.Scale3     = default;
        root            = database.LoadGameEntity(10L, out _);
        Test_ComponentReader.AssertRootEntity(root);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);
        
        // --- read child DatabaseEntity again
        child.Position  = default;
        child.Scale3    = default;
        child           = database.LoadGameEntity(11L, out _);
        Test_ComponentReader.AssertChildEntity(child);
        AreEqual(2,     type.EntityCount);
        AreEqual(2,     store.EntityCount);


    }
    
    [Test]
    public static void Test_Client_write_components()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        
        var database    = CreateClientDatabase(store);

        var entity  = store.CreateEntity(10);
        var child   = store.CreateEntity(11);
        entity.AddChild(child);
        entity.AddComponent(new Position { x = 1, y = 2, z = 3 });
        entity.AddBehavior(new TestBehavior1 { val1 = 10 });
        
        var ge = database.StoreGameEntity(entity);
        AreEqual(1, database.Entities.Count);
        
        AreEqual(10,    ge.pid);
        AreEqual(1,     ge.children.Count);
        AreEqual(11,    ge.children[0]);
        AreEqual("{\"pos\":{\"x\":1,\"y\":2,\"z\":3},\"testRef1\":{\"val1\":10}}", ge.components.AsString());
        
        ge = database.StoreGameEntity(child);
        AreEqual(2, database.Entities.Count);
        
        AreEqual(11,    ge.pid);
        IsNull  (ge.children);
        IsTrue  (ge.components.IsNull());
        
        int n = 0; 
        foreach (var e in database.Entities) {
            n++;
            NotNull(e.Value);
        }
        AreEqual(2, n);
    }
}