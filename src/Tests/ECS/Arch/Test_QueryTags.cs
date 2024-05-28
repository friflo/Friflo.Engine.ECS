using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_QueryTags
{
    [Test]
    public static void Test_Tags_AllTags()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.AllTags(Tags.Get<TestTag>());
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = query.AllTags(Tags.Get<TestTag, TestTag2>());
        AreEqual("7, 8, 9, 10",     query.Ids());
        
        query = query.AllTags(Tags.Get<TestTag, TestTag2, TestTag3>());
        AreEqual("8, 9, 10",        query.Ids());
        
        query = query.AllTags(Tags.Get<TestTag, TestTag2, TestTag3, TestTag4>());
        AreEqual("9, 10",           query.Ids());

        query = query.AllTags(Tags.Get<TestTag, TestTag2, TestTag3, TestTag4, TestTag5>());
        AreEqual("10",              query.Ids());
    }
    
    [Test]
    public static void Test_Tags_AnyTags()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.AnyTags(Tags.Get<TestTag>());
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyTags(Tags.Get<TestTag2>());
        AreEqual("3, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyTags(Tags.Get<TestTag3>());
        AreEqual("4, 8, 9, 10",     query.Ids());
        
        query = query.AnyTags(Tags.Get<TestTag4>());
        AreEqual("5, 9, 10",        query.Ids());

        query = query.AnyTags(Tags.Get<TestTag5>());
        AreEqual("6, 10",           query.Ids());
    }

    [Test]
    public static void Test_Tags_AnyTags_AllTags()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);

        var allTags = Tags.Get<TestTag2, TestTag3>(); // entities: 8, 9, 10
        
        query = query.AnyTags(Tags.Get<TestTag>()).AllTags(allTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyTags(Tags.Get<TestTag2>()).AllTags(allTags);
        AreEqual("3, 7, 8, 9, 10",  query.Ids());
        
        query = query.AnyTags(Tags.Get<TestTag3>()).AllTags(allTags);
        AreEqual("4, 8, 9, 10",     query.Ids());
        
        query = query.AnyTags(Tags.Get<TestTag4>()).AllTags(allTags);
        AreEqual("5, 8, 9, 10",     query.Ids());

        query = query.AnyTags(Tags.Get<TestTag5>()).AllTags(allTags);
        AreEqual("6, 8, 9, 10",     query.Ids());
    }
    
    [Test]
    public static void Test_Tags_WithoutAllTags()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.WithoutAllTags(Tags.Get<TestTag>());
        AreEqual("1, 3, 4, 5, 6",               query.Ids());
        
        query = query.WithoutAllTags(Tags.Get<TestTag, TestTag2>());
        AreEqual("1, 2, 3, 4, 5, 6",            query.Ids());
        
        query = query.WithoutAllTags(Tags.Get<TestTag, TestTag2, TestTag3>());
        AreEqual("1, 2, 3, 4, 5, 6, 7",         query.Ids());
        
        query = query.WithoutAllTags(Tags.Get<TestTag, TestTag2, TestTag3, TestTag4>());
        AreEqual("1, 2, 3, 4, 5, 6, 7, 8",      query.Ids());

        query = query.WithoutAllTags(Tags.Get<TestTag, TestTag2, TestTag3, TestTag4, TestTag5>());
        AreEqual("1, 2, 3, 4, 5, 6, 7, 8, 9",   query.Ids());
    }
    
    [Test]
    public static void Test_Tags_WithoutAnyTags()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        query = query.WithoutAnyTags(Tags.Get<TestTag>()).WithDisabled();
        AreEqual("1, 3, 4, 5, 6",           query.Ids());
        
        query = query.WithoutAnyTags(Tags.Get<TestTag2>());
        AreEqual("1, 2, 4, 5, 6",           query.Ids());
        
        query = query.WithoutAnyTags(Tags.Get<TestTag3>());
        AreEqual("1, 2, 3, 5, 6, 7",        query.Ids());
        
        query = query.WithoutAnyTags(Tags.Get<TestTag4>());
        AreEqual("1, 2, 3, 4, 6, 7, 8",     query.Ids());

        query = query.WithoutAnyTags(Tags.Get<TestTag5>());
        AreEqual("1, 2, 3, 4, 5, 7, 8, 9",  query.Ids());
    }
    
    [Test]
    public static void Test_Tags_WithoutAnyTags_WithoutAllTags()
    {
        var store   = CreateTestStore();
        var sig     = Signature.Get<Position>();
        var query   = store.Query(sig);
        
        var allTags = Tags.Get<TestTag2, TestTag3>(); // entities: 8, 9, 10
        
        // without: 1, 3, 4, 5, 6
        query = query.WithoutAnyTags(Tags.Get<TestTag>()).WithoutAllTags(allTags).WithDisabled();
        AreEqual("1, 3, 4, 5, 6",           query.Ids());
        
        // without: 1, 2, 4, 5, 6
        query = query.WithoutAnyTags(Tags.Get<TestTag2>()).WithoutAllTags(allTags);
        AreEqual("1, 2, 4, 5, 6",           query.Ids());
        
        // without: 1, 2, 3, 5, 6, 7
        query = query.WithoutAnyTags(Tags.Get<TestTag3>()).WithoutAllTags(allTags);
        AreEqual("1, 2, 3, 5, 6, 7",        query.Ids());
        
        // without: 1, 2, 3, 4, 6, 7, 8
        query = query.WithoutAnyTags(Tags.Get<TestTag4>()).WithoutAllTags(allTags);
        AreEqual("1, 2, 3, 4, 6, 7",        query.Ids());

        // without: 1, 2, 3, 4, 5, 7, 8, 9
        query = query.WithoutAnyTags(Tags.Get<TestTag5>()).WithoutAllTags(allTags);
        AreEqual("1, 2, 3, 4, 5, 7",        query.Ids());
    }
    
    private static EntityStore CreateTestStore()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        CreateEntity(store,  1, new Tags());    
        
        CreateEntity(store,  2, Tags.Get<TestTag>());
        CreateEntity(store,  3, Tags.Get<TestTag2>());
        CreateEntity(store,  4, Tags.Get<TestTag3>());
        CreateEntity(store,  5, Tags.Get<TestTag4>());
        CreateEntity(store,  6, Tags.Get<TestTag5>());
        
        CreateEntity(store,  7, Tags.Get<TestTag, TestTag2>());
        CreateEntity(store,  8, Tags.Get<TestTag, TestTag2, TestTag3>());
        CreateEntity(store,  9, Tags.Get<TestTag, TestTag2, TestTag3, TestTag4>());
        CreateEntity(store, 10, Tags.Get<TestTag, TestTag2, TestTag3, TestTag4, TestTag5>());
        return store;
    }
    
    private static void CreateEntity(EntityStore store, int id, in Tags tags)
    {
        var entity = store.CreateEntity(id);
        entity.AddComponent<Position>();
        entity.AddComponent<Rotation>();
        entity.AddComponent<EntityName>();
        entity.AddComponent<Scale3>();
        entity.AddComponent<MyComponent1>();
        entity.AddTags(tags);
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
    public static void Test_Tags_overloads_AllTags_AnyTags()
    {
        var store = CreateTestStore();
        
        var allTags = Tags.Get<TestTag2, TestTag3>(); // entities: 8, 9, 10
        var anyTags = Tags.Get<TestTag>();
        
        var query = store.Query().AllTags(allTags).AnyTags(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = store.Query<Position, Rotation>().AllTags(allTags).AnyTags(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
    
        query = store.Query<Position, Rotation, EntityName>().AllTags(allTags).AnyTags(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());

        query = store.Query<Position, Rotation, EntityName, Scale3>().AllTags(allTags).AnyTags(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
        
        query = store.Query<Position, Rotation, EntityName, Scale3, MyComponent1>().AllTags(allTags).AnyTags(anyTags);
        AreEqual("2, 7, 8, 9, 10",  query.Ids());
    }
    
    [Test]
    public static void Test_Tags_overloads_WithoutAllTags_WithoutAnyTags()
    {
        var store = CreateTestStore();
        
        var allTags = Tags.Get<TestTag2, TestTag3>(); // entities: 8, 9, 10
        var anyTags = Tags.Get<TestTag>();
        
        var query = store.Query().WithoutAllTags(allTags).WithoutAnyTags(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
        
        query = store.Query<Position, Rotation>().WithoutAllTags(allTags).WithoutAnyTags(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
    
        query = store.Query<Position, Rotation, EntityName>().WithoutAllTags(allTags).WithoutAnyTags(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());

        query = store.Query<Position, Rotation, EntityName, Scale3>().WithoutAllTags(allTags).WithoutAnyTags(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
        
        query = store.Query<Position, Rotation, EntityName, Scale3, MyComponent1>().WithoutAllTags(allTags).WithoutAnyTags(anyTags);
        AreEqual("1, 3, 4, 5, 6",  query.Ids());
    }
}

}