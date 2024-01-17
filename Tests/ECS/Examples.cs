using System;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable MemberCanBePrivate.Global
namespace Tests.ECS;

public static class Examples
{
    public struct MyComponent : IComponent {
        public int value;
    };
    
    [Test]
    public static void AddComponents()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.AddComponent(new EntityName("Hello World!"));
        entity.AddComponent(new MyComponent { value = 42 });
        Console.WriteLine($"entity: {entity}"); // entity: id: 1  "Hello World!"  [EntityName, Position]
    }

    public struct MyTag1 : ITag { };
    public struct MyTag2 : ITag { };

    [Test]
    public static void AddTags()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.AddTag<MyTag1>();
        entity.AddTag<MyTag2>();
        Console.WriteLine($"entity: {entity}"); // entity: id: 1  [#MyTag1, #MyTag2]
    }
    
    public class MyScript : Script { } 
    
    [Test]
    public static void AddScript()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.AddScript(new MyScript());
        Console.WriteLine($"entity: {entity}"); // entity: id: 1  [*MyScript]
    }
    
    [Test]
    public static void AddChildEntities()
    {
        var store   = new EntityStore();
        var root    = store.CreateEntity();
        var child1  = store.CreateEntity();
        var child2  = store.CreateEntity();
        root.AddChild(child1);
        root.AddChild(child2);
        Console.WriteLine($"child entities: {root.ChildEntities}"); // child entities: Count: 2
    }
    
    [Test]
    public static void AddEventHandlers()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.OnComponentChanged     += args => { Console.WriteLine(args); }; // entity: 1 - event > Add Component: [MyComponent]
        entity.OnTagsChanged          += args => { Console.WriteLine(args); }; // entity: 1 - event > Add Tags: [#MyTag1]
        entity.OnScriptChanged        += args => { Console.WriteLine(args); }; // entity: 1 - event > Add Script: [*MyScript]
        entity.OnChildEntitiesChanged += args => { Console.WriteLine(args); }; // entity: 1 - event > Add Child[0] = 2

        entity.AddComponent(new MyComponent());
        entity.AddTag<MyTag1>();
        entity.AddScript(new MyScript());
        entity.AddChild(store.CreateEntity());
    }
    
}