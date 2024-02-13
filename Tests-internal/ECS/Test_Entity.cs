using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Entity
{
    [Test]
    public static void Test_Entity_Components()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        
        AreEqual(0, entity.Components.Length);
        
        entity.AddComponent(new Position(1, 2, 3));
        entity.AddComponent(new EntityName("test"));
       
        var components = entity.Components;
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
        
        AreEqual(0, entity.Children.Length);
        
        entity.AddChild(child1);
        entity.AddChild(child2);
        child1.AddChild(sub11);
        child1.AddChild(sub12);
        child2.AddChild(sub21);
        
        var children = entity.Children;
        AreEqual(2, children.Length);
        AreEqual(child1, children[0]);
        AreEqual(child2, children[1]);
        
        AreEqual(2, child1.Children.Length);
        AreEqual(1, child2.Children.Length);
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
}

