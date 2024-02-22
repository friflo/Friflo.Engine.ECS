using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Entity
{
    [Test]
    public static void Test_Entity_Components()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        var components = entity.Components.GetComponentArray(); 
        AreEqual(0, components.Length);
        
        entity.AddComponent(new Position(1, 2, 3));
        entity.AddComponent(new EntityName("test"));
       
        components = entity.Components.GetComponentArray();
        AreEqual(2, components.Length);
        AreEqual("test",                ((EntityName)components[0]).value);
        AreEqual(new Position(1,2,3),   (Position)components[1]);
    }
    
    
    [Test]
    public static void Test_Entity_Children()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        var child1 = store.CreateEntity();
        var child2 = store.CreateEntity();
        var sub11   = store.CreateEntity();
        var sub12   = store.CreateEntity();
        var sub21   = store.CreateEntity();
        
        AreEqual(0, entity.ChildEntities.ToArray().Length);
        
        entity.AddChild(child1);
        entity.AddChild(child2);
        child1.AddChild(sub11);
        child1.AddChild(sub12);
        child2.AddChild(sub21);
        
        var children = entity.ChildEntities.ToArray();
        AreEqual(2, children.Length);
        AreEqual(child1, children[0]);
        AreEqual(child2, children[1]);
        
        AreEqual(2, child1.ChildEntities.Count);
        AreEqual(1, child2.ChildEntities.Count);
    }
    
    [Test]
    public static void Test_Entity_Info()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        
        entity.AddComponent<Position>();
        entity.AddScript(new TestScript1());
        entity.AddChild(store.CreateEntity());
        entity.AddChild(store.CreateEntity());
        
        var json =
"""
{
    "id": 1,
    "children": [
        2,
        3
    ],
    "components": {
        "pos": {"x":0,"y":0,"z":0},
        "script1": {"val1":0}
    }
}
""";
        AreEqual("",                            entity.Info.ToString());
        AreEqual(entity.Pid,                    entity.Info.Pid);
        AreEqual(json,                          entity.Info.JSON);
        AreEqual("event types: 0, handlers: 0", entity.Info.EventHandlers.ToString());
    }
    
    [Test]
    public static void Test_Entity_debugger_screenshot()
    {
        var store   = new EntityStore();
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("root"));
        
        var entity  = store.CreateEntity();
        root.AddChild(entity);

        entity.AddComponent(new EntityName("hello entity"));
        entity.AddComponent(new Position(10, 10, 0));
        entity.AddTag<MyTag>();
        var child1 = store.CreateEntity();
        var child2 = store.CreateEntity();
        var child3 = store.CreateEntity();
        child1.AddComponent(new Position(1, 1, 0));
        child2.AddComponent(new Position(1, 1, 1));
        child3.AddComponent(new Position(1, 1, 2));
        child3.AddTag<MyTag>();
            
        entity.AddChild(child1);
        entity.AddChild(child2);
        entity.AddChild(child3);
        
        DebuggerEntityScreenshot(entity);
    }
    
    private static void DebuggerEntityScreenshot(Entity entity) {
        _ = entity;
    }
    
    [Test]
    public static void Test_Entity_EntityComponents()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent<Position>();
        entity.AddComponent<Rotation>();
        
        var debugView   = new EntityComponentsDebugView(entity.Components);
        var components  = debugView.Components;
        
        AreEqual(2, components.Length);
        AreEqual(new Position(), components[0]);
        AreEqual(new Rotation(), components[1]);
    }
    
    [Test]
    public static void Test_Entity_ChildEntities_DebugView()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity(1);
        entity.AddChild(store.CreateEntity(2));
        entity.AddChild(store.CreateEntity(3));
        
        var debugView   = new ChildEntitiesDebugView(entity.ChildEntities);
        var entities    = debugView.Entities;
        
        AreEqual(2, entities.Length);
        AreEqual(2, entities[0].Id);
        AreEqual(3, entities[1].Id);
    }
    
    [Test]
    public static void Test_Entity_Scripts_DebugView()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        var script = new TestScript1();
        entity.AddScript(script);
        
        var debugView   = new ScriptsDebugView(entity.Scripts);
        var scripts     = debugView.Items;
        
        AreEqual(1,     scripts .Length);
        AreSame(script, scripts[0]);
    }
    
    [Test]
    public static void Test_Tags_Disable_Enable()
    {
        var count       = 10;    // 1_000_000 ~ #PC: 8475 ms
        var entityCount = 100;
        var store       = new EntityStore(PidType.UsePidAsId);
        var root        = store.CreateEntity();
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        
        for (int n = 1; n < entityCount; n++) {
            root.AddChild(archetype.CreateEntity());
        }
        IsTrue (root.Enabled);
        
        var sw = new Stopwatch();
        sw.Start();
        long start = 0;
        for (int i = 0; i < count; i++)
        {
            root.EnableTree(true);
            root.EnableTree(false);
            if (i == 0) start = Mem.GetAllocatedBytes();
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"Disable / Enable - duration: {sw.ElapsedMilliseconds} ms");
        
        var query = store.Query().AllTags(Tags.Get<Disabled>());
        AreEqual(entityCount, query.Count);
        IsFalse (root.Enabled);
    }
}

internal struct MyTag : ITag { }

