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
        AssertResult("2, 7, 8, 9, 10",  query);
        
        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2>());
        AssertResult("7, 8, 9, 10",     query);
        
        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3>());
        AssertResult("8, 9, 10",        query);
        
        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4>());
        AssertResult("9, 10",           query);

        query = query.AllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4, Comp5>());
        AssertResult("10",              query);
    }
    
    [Test]
    public static void Test_QueryComponents_AnyComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp1>());
        AssertResult("2, 7, 8, 9, 10",  query);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp2>());
        AssertResult("3, 7, 8, 9, 10",  query);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp3>());
        AssertResult("4, 8, 9, 10",     query);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp4>());
        AssertResult("5, 9, 10",        query);

        query = query.AnyComponents(ComponentTypes.Get<Comp5>());
        AssertResult("6, 10",           query);
    }

    [Test]
    public static void Test_QueryComponents_AnyComponents_AllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);

        var all = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        
        query = query.AnyComponents(ComponentTypes.Get<Comp1>()).AllComponents(all);
        AssertResult("2, 7, 8, 9, 10",  query);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp2>()).AllComponents(all);
        AssertResult("3, 7, 8, 9, 10",  query);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp3>()).AllComponents(all);
        AssertResult("4, 8, 9, 10",     query);
        
        query = query.AnyComponents(ComponentTypes.Get<Comp4>()).AllComponents(all);
        AssertResult("5, 8, 9, 10",     query);

        query = query.AnyComponents(ComponentTypes.Get<Comp5>()).AllComponents(all);
        AssertResult("6, 8, 9, 10",     query);
    }
    
    [Test]
    public static void Test_QueryComponents_WithoutAllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1>());
        AssertResult("1, 3, 4, 5, 6",               query);
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2>());
        AssertResult("1, 2, 3, 4, 5, 6",            query);
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3>());
        AssertResult("1, 2, 3, 4, 5, 6, 7",         query);
        
        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4>());
        AssertResult("1, 2, 3, 4, 5, 6, 7, 8",      query);

        query = query.WithoutAllComponents(ComponentTypes.Get<Comp1, Comp2, Comp3, Comp4, Comp5>());
        AssertResult("1, 2, 3, 4, 5, 6, 7, 8, 9",   query);
    }
    
    [Test]
    public static void Test_QueryComponents_WithoutAnyComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp1>());
        AssertResult("1, 3, 4, 5, 6",           query);
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp2>());
        AssertResult("1, 2, 4, 5, 6",           query);
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp3>());
        AssertResult("1, 2, 3, 5, 6, 7",        query);
        
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp4>());
        AssertResult("1, 2, 3, 4, 6, 7, 8",     query);

        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp5>());
        AssertResult("1, 2, 3, 4, 5, 7, 8, 9",  query);
    }
    
    [Test]
    public static void Test_QueryComponents_WithoutAnyComponents_WithoutAllComponents()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        var all = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        
        // without: 1, 3, 4, 5, 6
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp1>()).WithoutAllComponents(all);
        AssertResult("1, 3, 4, 5, 6",           query);
        
        // without: 1, 2, 4, 5, 6
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp2>()).WithoutAllComponents(all);
        AssertResult("1, 2, 4, 5, 6",           query);
        
        // without: 1, 2, 3, 5, 6, 7
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp3>()).WithoutAllComponents(all);
        AssertResult("1, 2, 3, 5, 6, 7",        query);
        
        // without: 1, 2, 3, 4, 6, 7, 8
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp4>()).WithoutAllComponents(all);
        AssertResult("1, 2, 3, 4, 6, 7",        query);

        // without: 1, 2, 3, 4, 5, 7, 8, 9
        query = query.WithoutAnyComponents(ComponentTypes.Get<Comp5>()).WithoutAllComponents(all);
        AssertResult("1, 2, 3, 4, 5, 7",        query);
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
    
    private static void AssertResult(string expect, ArchetypeQuery query)
    {
        var list = new List<int>();
        foreach (var entity in query.Entities) {
            list.Add(entity.Id);
        }
        var ids = string.Join(", ", list);
        
        AreEqual(expect, ids);
        
        foreach (var entity in query.Store.Entities)
        {
            var archetype       = entity.Archetype;
            var queryContains   = query.IsMatch(archetype.ComponentTypes, archetype.Tags);
            var listContains    = list.Contains(entity.Id);
            AreEqual(listContains, queryContains);
        }
    }

    [Test]
    public static void Test_QueryComponents_overloads_AllComponents_AnyComponents()
    {
        var store = CreateTestStore();
        
        var all = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        var any = ComponentTypes.Get<Comp1>();
        
        var query = store.Query().AllComponents(all).AnyComponents(any);
        AssertResult("2, 7, 8, 9, 10",  query);
        
        query = store.Query<Position, Rotation>().AllComponents(all).AnyComponents(any);
        AssertResult("2, 7, 8, 9, 10",  query);
    
        query = store.Query<Position, Rotation, EntityName>().AllComponents(all).AnyComponents(any);
        AssertResult("2, 7, 8, 9, 10",  query);

        query = store.Query<Position, Rotation, EntityName, Scale3>().AllComponents(all).AnyComponents(any);
        AssertResult("2, 7, 8, 9, 10",  query);
        
        query = store.Query<Position, Rotation, EntityName, Scale3, MyComponent1>().AllComponents(all).AnyComponents(any);
        AssertResult("2, 7, 8, 9, 10",  query);
    }
    
    [Test]
    public static void Test_QueryComponents_overloads_WithoutAllComponents_WithoutAnyComponents()
    {
        var store = CreateTestStore();
        
        var all = ComponentTypes.Get<Comp2, Comp3>(); // entities: 8, 9, 10
        var any = ComponentTypes.Get<Comp1>();
        
        var query = store.Query().WithoutAllComponents(all).WithoutAnyComponents(any);
        AssertResult("1, 3, 4, 5, 6",  query);
        
        query = store.Query<Position, Rotation>().WithoutAllComponents(all).WithoutAnyComponents(any);
        AssertResult("1, 3, 4, 5, 6",  query);
    
        query = store.Query<Position, Rotation, EntityName>().WithoutAllComponents(all).WithoutAnyComponents(any);
        AssertResult("1, 3, 4, 5, 6",  query);

        query = store.Query<Position, Rotation, EntityName, Scale3>().WithoutAllComponents(all).WithoutAnyComponents(any);
        AssertResult("1, 3, 4, 5, 6",  query);
        
        query = store.Query<Position, Rotation, EntityName, Scale3, MyComponent1>().WithoutAllComponents(all).WithoutAnyComponents(any);
        AssertResult("1, 3, 4, 5, 6",  query);
    }
    
    [Test]
    public static void Test_QueryComponents_Query_IsMatch_components()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        
        var query = store.Query();
        IsTrue(query.IsMatch(default, default));
        
        query = store.Query().AllComponents(ComponentTypes.Get<Comp1, Comp2>());
        IsFalse(query.IsMatch(ComponentTypes.Get<Comp1>(),                  default));
        IsTrue (query.IsMatch(ComponentTypes.Get<Comp1, Comp2>(),           default));
        IsTrue (query.IsMatch(ComponentTypes.Get<Comp1, Comp2, Comp3>(),    default));
        
        query = store.Query<Position>().AllComponents(ComponentTypes.Get<Comp1, Comp2>());
        IsFalse(query.IsMatch(ComponentTypes.Get<Comp1>(),                  default));
        IsFalse(query.IsMatch(ComponentTypes.Get<Comp1, Comp2>(),           default));
        IsFalse(query.IsMatch(ComponentTypes.Get<Comp1, Comp2, Comp3>(),    default));
    }
    
    [Test]
    public static void Test_QueryComponents_Query_IsMatch_tags()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        
        var query = store.Query();
        IsTrue(query.IsMatch(default, default));
        
        query = store.Query().AllTags(Tags.Get<TestTag, TestTag2>());
        IsFalse(query.IsMatch(default, Tags.Get<TestTag>()));
        IsTrue (query.IsMatch(default, Tags.Get<TestTag, TestTag2>()));
        IsTrue (query.IsMatch(default, Tags.Get<TestTag, TestTag2, TestTag3>()));
    }
    
    [Test]
    public static void Test_QueryComponents_QueryFilter_components()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var query = store.Query()
            .AllComponents       (ComponentTypes.Get<Comp1>())
            .AnyComponents       (ComponentTypes.Get<Comp2>())
            .WithoutAllComponents(ComponentTypes.Get<Comp3>())
            .WithoutAnyComponents(ComponentTypes.Get<Comp4>());
        
        var filter = query.Filter;
        AreEqual(ComponentTypes.Get<Comp1>(), filter.Condition.AllComponents);
        AreEqual(ComponentTypes.Get<Comp2>(), filter.Condition.AnyComponents);
        AreEqual(ComponentTypes.Get<Comp3>(), filter.Condition.WithoutAllComponents);
        AreEqual(ComponentTypes.Get<Comp4>(), filter.Condition.WithoutAnyComponents);
    }
    
    [Test]
    public static void Test_QueryComponents_QueryFilter_tags()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        
        var query   = store.Query().AllTags       (Tags.Get<TestTag>());
        AreEqual(Tags.Get<TestTag>(),           query.Filter.Condition.AllTags);
            
        query       = store.Query().AnyTags       (Tags.Get<TestTag2>());
        AreEqual(Tags.Get<TestTag2>(),          query.Filter.Condition.AnyTags);
        
        query       = store.Query().WithoutAllTags(Tags.Get<TestTag3>());
        AreEqual(Tags.Get<TestTag3>(),          query.Filter.Condition.WithoutAllTags);
        
        query       = store.Query().WithoutAnyTags(Tags.Get<TestTag4>());
        AreEqual(Tags.Get<TestTag4,Disabled>(), query.Filter.Condition.WithoutAnyTags);
    }
}

}