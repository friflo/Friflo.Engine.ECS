using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_QueryComponents
{
    private struct Comp1 : IComponent { }
    private struct Comp2 : IComponent { }
    private struct Comp3 : IComponent { }
    private struct Comp4 : IComponent { }
    private struct Comp5 : IComponent { }
    
    [Test]
    public static void Test_QueryComponents_AllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.AllComponents(ComponentTypes.Get<Comp1>());
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2>());
        AreEqual("7, 8, 9, 10",     query.Ids());
        
        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3>());
        AreEqual("8, 9, 10",        query.Ids());
        
        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4>());
        AreEqual("9, 10",           query.Ids());

        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4, Comp5>());
        AreEqual("10",              query.Ids());
    }
    
    [Test]
    public static void Test_QueryComponents_AnyComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp1>());
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyComponents(ComponentTypes.Get<Comp2>());
        AreEqual("3, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyComponents(ComponentTypes.Get<Comp3>());
        AreEqual("4, 8, 9, 10",     query.Ids());
        
        query = query.AnyComponents(ComponentTypes.Get<Comp4>());
        AreEqual("5, 9, 10",        query.Ids());

        query = query.AnyComponents(ComponentTypes.Get<Comp5>());
        AreEqual("6, 10",           query.Ids());
    }

    [Test]
    public static void Test_QueryComponents_AnyComponents_AllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);

        var allTags = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        
        query = query.AnyComponents(ComponentTypes.Get<Comp1>()).AllComponents(allTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyComponents(ComponentTypes.Get<Comp2>()).AllComponents(allTags);
        AreEqual("3, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyComponents(ComponentTypes.Get<Comp3>()).AllComponents(allTags);
        AreEqual("4, 8, 9, 10",     query.Ids());
        
        query = query.AnyComponents(ComponentTypes.Get<Comp4>()).AllComponents(allTags);
        AreEqual("5, 8, 9, 10",     query.Ids());

        query = query.AnyComponents(ComponentTypes.Get<Comp5>()).AllComponents(allTags);
        AreEqual("6, 8, 9, 10",     query.Ids());
    }
    
    [Test]
    public static void Test_QueryComponents_WithoutAllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1>());
        AreEqual("1, 3, 4, 5, 6",               query.Ids());
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2>());
        AreEqual("1, 2, 3, 4, 5, 6",            query.Ids());
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3>());
        AreEqual("1, 2, 3, 4, 5, 6, 7",         query.Ids());
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4>());
        AreEqual("1, 2, 3, 4, 5, 6, 7, 8",      query.Ids());

        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4, Comp5>());
        AreEqual("1, 2, 3, 4, 5, 6, 7, 8, 9",   query.Ids());
    }
    
    [Test]
    public static void Test_QueryComponents_WithoutAnyComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp1>());
        AreEqual("1, 3, 4, 5, 6",           query.Ids());
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp2>());
        AreEqual("1, 2, 4, 5, 6",           query.Ids());
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp3>());
        AreEqual("1, 2, 3, 5, 6, 7",        query.Ids());
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp4>());
        AreEqual("1, 2, 3, 4, 6, 7, 8",     query.Ids());

        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp5>());
        AreEqual("1, 2, 3, 4, 5, 7, 8, 9",  query.Ids());
    }
    
    [Test]
    public static void Test_QueryComponents_WithoutAnyComponents_WithoutAllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        var allTags = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        
        // without: 1, 3, 4, 5, 6
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp1>()).WithoutAllComponents(allTags);
        AreEqual("1, 3, 4, 5, 6",           query.Ids());
        
        // without: 1, 2, 4, 5, 6
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp2>()).WithoutAllComponents(allTags);
        AreEqual("1, 2, 4, 5, 6",           query.Ids());
        
        // without: 1, 2, 3, 5, 6, 7
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp3>()).WithoutAllComponents(allTags);
        AreEqual("1, 2, 3, 5, 6, 7",        query.Ids());
        
        // without: 1, 2, 3, 4, 6, 7, 8
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp4>()).WithoutAllComponents(allTags);
        AreEqual("1, 2, 3, 4, 6, 7",        query.Ids());

        // without: 1, 2, 3, 4, 5, 7, 8, 9
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp5>()).WithoutAllComponents(allTags);
        AreEqual("1, 2, 3, 4, 5, 7",        query.Ids());
    }
    
    private static EntityStore CreateTestStore()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        CreateEntity(store,  1, new ComponentTypes());    
        
        CreateEntity(store,  2, ComponentTypes.Get<Comp1>());
        CreateEntity(store,  3, ComponentTypes.Get<Comp2>());
        CreateEntity(store,  4, ComponentTypes.Get<Comp3>());
        CreateEntity(store,  5, ComponentTypes.Get<Comp4>());
        CreateEntity(store,  6, ComponentTypes.Get<Comp5>());
        
        CreateEntity(store,  7, ComponentTypes.Get<Comp1, Comp2>());
        CreateEntity(store,  8, ComponentTypes.Get<Comp1, Comp2, Comp3>());
        CreateEntity(store,  9, ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4>());
        CreateEntity(store, 10, ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4, Comp5>());
        return store;
    }
    
    private static void CreateEntity(EntityStore store, int id, in ComponentTypes componentTypes)
    {
        var entity = store.CreateEntity(id);
        entity.AddComponent<Position>();
        entity.AddComponent<Rotation>();
        entity.AddComponent<EntityName>();
        entity.AddComponent<Scale3>();
        entity.AddComponent<MyComponent1>();
        foreach (var componentType in componentTypes) {
            EntityUtils.AddEntityComponent(entity, componentType);            
        }
    }
    
    private static string Ids(this ArchetypeQuery entities)
    {
        var list = new List<int>();
        foreach (var entity in entities.Entities) {
            list.Add(entity.Id);
        }
        return string.Join(", ", list);
    }
    

    [Test]
    public static void Test_QueryComponents_overloads_AllComponents_AnyComponents()
    {
        var store = CreateTestStore();
        
        var allTags = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        var anyTags = ComponentTypes.Get<Comp1>();
        
        var query = store.Query().AllComponents(allTags).AnyComponents(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = store.Query<Position, Rotation>().AllComponents(allTags).AnyComponents(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
    
        query = store.Query<Position, Rotation, EntityName>().AllComponents(allTags).AnyComponents(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());

        query = store.Query<Position, Rotation, EntityName, Scale3>().AllComponents(allTags).AnyComponents(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = store.Query<Position, Rotation, EntityName, Scale3, MyComponent1>().AllComponents(allTags).AnyComponents(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
    }
    
    [Test]
    public static void Test_QueryComponents_overloads_WithoutAllComponents_WithoutAnyComponents()
    {
        var store = CreateTestStore();
        
        var allTags = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        var anyTags = ComponentTypes.Get<Comp1>();
        
        var query = store.Query().WithoutAllComponents(allTags).WithoutAnyComponents(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
        
        query = store.Query<Position, Rotation>().WithoutAllComponents(allTags).WithoutAnyComponents(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
    
        query = store.Query<Position, Rotation, EntityName>().WithoutAllComponents(allTags).WithoutAnyComponents(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());

        query = store.Query<Position, Rotation, EntityName, Scale3>().WithoutAllComponents(allTags).WithoutAnyComponents(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
        
        query = store.Query<Position, Rotation, EntityName, Scale3, MyComponent1>().WithoutAllComponents(allTags).WithoutAnyComponents(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
    }
}

}