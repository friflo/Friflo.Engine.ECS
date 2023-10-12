using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS;

internal struct TestTag  : IEntityTag { }

internal struct TestTag2 : IEntityTag { }

internal struct TestTag3 : IEntityTag { }

public static class Test_Tags
{
    [Test]
    public static void Test_Tags_basics()
    {
        var twoTags = Tags.Get<TestTag, TestTag2>();
        AreEqual("Tags: [#TestTag, #TestTag2]", twoTags.ToString());
        
        var tags    = new Tags();
        AreEqual(0, tags.Count);
        AreEqual("Tags: []",                    tags.ToString());
        IsFalse(tags.Has<TestTag>());
        IsFalse(tags.HasAll(twoTags));
        IsFalse(tags.HasAny(twoTags));
        
        tags.Add<TestTag>();
        AreEqual(1, tags.Count);
        IsTrue (tags.Has<TestTag>());
        IsFalse(tags.HasAll(twoTags));
        IsTrue (tags.HasAny(twoTags));
        
        AreEqual("Tags: [#TestTag]",            tags.ToString());
        
        tags.Add<TestTag2>();
        AreEqual(2, tags.Count);
        AreEqual("Tags: [#TestTag, #TestTag2]", tags.ToString());
        IsTrue (tags.Has<TestTag, TestTag2>());
        IsFalse(tags.Has<TestTag, TestTag2, TestTag3>());
        IsTrue (tags.HasAll(twoTags));
        IsTrue (tags.HasAny(twoTags));

        var copy = new Tags();
        copy.Add(tags);
        AreEqual(2, tags.Count);
        AreEqual("Tags: [#TestTag, #TestTag2]", copy.ToString());
        
        copy.Remove<TestTag>();
        AreEqual(1, copy.Count);
        AreEqual("Tags: [#TestTag2]",           copy.ToString());
        
        copy.Remove(tags);
        AreEqual(0, copy.Count);
        AreEqual("Tags: []",                    copy.ToString());
        
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        AreEqual("Tags: []", entity.Tags.ToString());
    }
    
    [Test]
    public static void Test_Tags_generic_IEnumerator()
    {
        IEnumerable<ComponentType> tags = Tags.Get<TestTag>();
        int count = 0;
        foreach (var _ in tags) {
            count++;
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_TagsEnumerator()
    {
        var tags = Tags.Get<TestTag>();
        var enumerator = tags.GetEnumerator();
        int count = 0;
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(1, count);
        
        count = 0;
        enumerator.Reset();
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(1, count);
        enumerator.Dispose();
    }
    
    [Test]
    public static void Test_Tags_Get()
    {
        var schema          = EntityStore.GetComponentSchema();
        var testTagType     = schema.TagTypeByType[typeof(TestTag)];
        var testTagType2    = schema.TagTypeByType[typeof(TestTag2)];
        
        var tag1    = Tags.Get<TestTag>();
        AreEqual("Tags: [#TestTag]", tag1.ToString());
        int count1 = 0;
        foreach (var tagType in tag1) {
            AreSame(testTagType, tagType);
            count1++;
        }
        AreEqual(1, count1);
        
        var count2 = 0;
        var tag2 = Tags.Get<TestTag, TestTag2>();
        AreEqual("Tags: [#TestTag, #TestTag2]", tag2.ToString());
        foreach (var _ in tag2) {
            count2++;
        }
        AreEqual(2, count2);
        
        AreEqual(tag2, Tags.Get<TestTag2, TestTag>());
    }
    
    [Test]
    public static void Test_Tags_Get_Mem()
    {
        var tag1    = Tags.Get<TestTag>();
        foreach (var _ in tag1) { }
        
        // --- 1 tag
        var start   = Mem.GetAllocatedBytes();
        int count1 = 0;
        foreach (var _ in tag1) {
            count1++;
        }
        Mem.AssertNoAlloc(start);
        AreEqual(1, count1);
        
        // --- 2 tags
        start       = Mem.GetAllocatedBytes();
        var tag2    = Tags.Get<TestTag, TestTag2>();
        var count2 = 0;
        foreach (var _ in tag2) {
            count2++;
        }
        Mem.AssertNoAlloc(start);
        AreEqual(2, count2);
    }
    
    [Test]
    public static void Test_tagged_Query() {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        var sig1 = Signature.Get<Position>();
        var sig2 = Signature.Get<Position, Rotation>();
        var sig3 = Signature.Get<Position, Rotation, Scale3>();
        var sig4 = Signature.Get<Position, Rotation, Scale3, MyComponent1>();
        var sig5 = Signature.Get<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        //
        var query2 = store.Query(sig2).AllTags(Tags.Get<TestTag, TestTag2>());
    }
    
    [Test]
    public static void Test_Tags_Add_Remove() {
        var store       = new EntityStore();
        AreEqual(1,                                 store.Archetypes.Length);
        var entity      = store.CreateEntity();
        var testTag2    = Tags.Get<TestTag2>();
        
        entity.AddTag<TestTag>();
        AreEqual("[#TestTag]  Count: 1",            entity.Archetype.ToString());
        AreEqual(1,                                 store.EntityCount);
        AreEqual(2,                                 store.Archetypes.Length);
        
        entity.AddTags(testTag2);
        AreEqual("[#TestTag, #TestTag2]  Count: 1",  entity.Archetype.ToString());
        AreEqual(1,                                 store.EntityCount);
        AreEqual(3,                                 store.Archetypes.Length);
        
        entity.RemoveTag<TestTag>();
        AreEqual("[#TestTag2]  Count: 1",           entity.Archetype.ToString());
        AreEqual(1,                                 store.EntityCount);
        AreEqual(4,                                 store.Archetypes.Length);
        
        entity.RemoveTags(testTag2);
        AreEqual("[]",                              entity.Archetype.ToString());
        AreEqual(1,                                 store.EntityCount);
        AreEqual(4,                                 store.Archetypes.Length);
        
        // Execute previous operations again. All required archetypes are now present
        const int count = 10; // 10_000_000 ~ 1.349 ms
        var start = Mem.GetAllocatedBytes();
        // each tags mutation causes a structural change
        for (int n = 0; n < count; n++) {
            entity.AddTag       <TestTag>();
            entity.AddTags      (testTag2);
            entity.RemoveTag    <TestTag>();
            entity.RemoveTags   (testTag2);
        }
        Mem.AssertNoAlloc(start);
        AreEqual(1,                                 store.EntityCount);
        AreEqual(4,                                 store.Archetypes.Length);
    }
    
    [Test]
    public static void Test_Tags_Query() {
        var store           = new EntityStore();
        var archTestTag     = store.GetArchetype(Tags.Get<TestTag>());
        var archTestTagAll  = store.GetArchetype(Tags.Get<TestTag, TestTag2>());
        AreEqual(3,                                store.Archetypes.Length);
        
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();
        entity2.AddTag<TestTag2>();
        AreEqual("[#TestTag]  Count: 1",            entity1.Archetype.ToString());
        AreEqual("[#TestTag, #TestTag2]  Count: 1", entity2.Archetype.ToString());
        AreEqual(2,                                 store.EntityCount);
        AreEqual(3,                                 store.Archetypes.Length);
        AreEqual(1,                                 archTestTag.EntityCount);
        AreEqual(1,                                 archTestTagAll.EntityCount);
        {
            var query  = store.Query().AllTags(Tags.Get<TestTag>());
            AreEqual("Query: [#TestTag]", query.ToString());
            int count   = 0;
            foreach (var id in query) {
                switch (count) {
                    case 0: AreEqual(1, id); break;
                    case 1: AreEqual(2, id); break;
                }
                count++;
            }
            AreEqual(2, count);
        } {
            var query  = store.Query().AllTags(Tags.Get<TestTag2>());
            AreEqual("Query: [#TestTag2]", query.ToString());
            int count   = 0;
            foreach (var id in query) {
                count++;
                AreEqual(2, id);
            }
            AreEqual(1, count);
        } { 
            var query = store.Query().AllTags(Tags.Get<TestTag, TestTag2>());
            AreEqual("Query: [#TestTag, #TestTag2]", query.ToString());
            int count   = 0;
            foreach (var id in query) {
                count++;
                AreEqual(2, id);
            }
            AreEqual(1, count);
        }
    }
}

