using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
// ReSharper disable UnusedVariable

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
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new EntityName("Hello World!")); // EntityName is a build-in component
        entity.AddComponent(new MyComponent { value = 42 });
        Console.WriteLine($"entity: {entity}");     // > entity: id: 1  "Hello World!"  [EntityName, Position]
    }
    
    /// <summary>
    /// <see cref="EntityStoreBase.GetUniqueEntity"/> is used to reduce code coupling.
    /// It enables access to a unique entity without the need to pass the entity by external code.   
    /// </summary>
    [Test]
    public static void GetUniqueEntity()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new UniqueEntity("Player")); // UniqueEntity is a build-in component
        
        var player  = store.GetUniqueEntity("Player");
        Console.WriteLine($"entity: {player}");     // entity: id: 1  [UniqueEntity]
    }

    public struct MyTag1 : ITag { };
    public struct MyTag2 : ITag { };

    [Test]
    public static void AddTags()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddTag<MyTag1>();
        entity.AddTag<MyTag2>();
        Console.WriteLine($"entity: {entity}");     // > entity: id: 1  [#MyTag1, #MyTag2]
    }
    
    public class MyScript : Script { } 
    
    [Test]
    public static void AddScript()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddScript(new MyScript());
        Console.WriteLine($"entity: {entity}");     // > entity: id: 1  [*MyScript]
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
        Console.WriteLine($"child entities: {root.ChildEntities}"); // > child entities: Count: 2
    }
    
    [Test]
    public static void AddEventHandlers()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.OnComponentChanged     += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Component: [MyComponent]
        entity.OnTagsChanged          += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Tags: [#MyTag1]
        entity.OnScriptChanged        += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Script: [*MyScript]
        entity.OnChildEntitiesChanged += ev => { Console.WriteLine(ev); }; // > entity: 1 - event > Add Child[0] = 2

        entity.AddComponent(new MyComponent());
        entity.AddTag<MyTag1>();
        entity.AddScript(new MyScript());
        entity.AddChild(store.CreateEntity());
    }
    
    public readonly struct MySignal { } 
    
    [Test]
    public static void AddSignalHandler()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddSignalHandler<MySignal>(signal => { Console.WriteLine(signal); }); // > entity: 1 - signal > MySignal    
        entity.EmitSignal(new MySignal());
    }
    
    [Test]
    public static void EntityQueries()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity();
        entity1.AddComponent(new EntityName("test"));
        entity1.AddTag<MyTag1>();
        
        var entity2 = store.CreateEntity();
        entity2.AddComponent(new MyComponent { value = 42 });
        entity2.AddTag<MyTag1>();
        
        var entity3 = store.CreateEntity();
        entity3.AddComponent(new MyComponent { value = 1337 });
        entity3.AddTag<MyTag1>();
        entity3.AddTag<MyTag2>();
        
        // --- query components
        var queryEntityNames = store.Query<EntityName>();
        Console.WriteLine(queryEntityNames);    // > Query: [EntityName]  EntityCount: 1

        var queryMyComponents = store.Query<MyComponent>();
        Console.WriteLine(queryMyComponents);   // > Query: [MyComponent]  EntityCount: 2
        
        // --- query tags
        var queryTag  = store.Query().AllTags(Tags.Get<MyTag1>());
        Console.WriteLine(queryTag);            // > Query: [#MyTag1]  EntityCount: 3
        
        var queryTags = store.Query().AllTags(Tags.Get<MyTag1, MyTag2>());
        Console.WriteLine(queryTags);           // > Query: [#MyTag1, #MyTag2]  EntityCount: 1
    }
    
    [Test]
    public static void EnumerateQueryChunks()
    {
        var store   = new EntityStore();
        for (int n = 0; n < 3; n++) {
            var entity = store.CreateEntity();
            entity.AddComponent(new MyComponent{ value = n + 42 });
        }
        var queryMyComponents = store.Query<MyComponent>();
        foreach (var (components, entities) in queryMyComponents.Chunks)
        {
            foreach (var component in components.Span) {
                Console.WriteLine($"MyComponent.value: {component.value}");
                // > MyComponent.value: 42
                // > MyComponent.value: 43
                // > MyComponent.value: 44
            }
        }
    }
}