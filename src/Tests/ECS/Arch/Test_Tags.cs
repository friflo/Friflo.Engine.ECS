using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {



public static class Test_Tags
{
    [Test]
    public static void Test_Tags_basics()
    {
        var twoTags = Tags.Get<TestTag, TestTag2>();
        AreEqual("Tags: [#TestTag, #TestTag2]", twoTags.ToString());
        
        var tags    = new Tags();
        AreEqual(0, tags.Count);
        IsFalse(tags.HasAny(tags));
        IsTrue (tags.HasAll(tags));
        
        AreEqual("Tags: []",                    tags.ToString());
        IsFalse(tags.Has<TestTag>());
        IsFalse(tags.HasAll(twoTags));
        IsFalse(tags.HasAny(twoTags));
        
        tags.Add<TestTag>();
        AreEqual("Tags: [#TestTag]",            tags.ToString());
        
        tags = Tags.Get<TestTag>();
        AreEqual(1, tags.Count);
        IsTrue (tags.Has<TestTag>());
        IsFalse(tags.HasAll(twoTags));
        IsTrue (tags.HasAny(twoTags));
        
        AreEqual("Tags: [#TestTag]",            tags.ToString());
        
        tags = Tags.Get<TestTag, TestTag2>();
        AreEqual(2, tags.Count);
        AreEqual("Tags: [#TestTag, #TestTag2]", tags.ToString());
        IsFalse(tags.Has<TestTag3, TestTag>());
        IsTrue (tags.Has<TestTag, TestTag2>());
        
        IsFalse(tags.Has<TestTag3, TestTag, TestTag2>());
        IsFalse(tags.Has<TestTag, TestTag2, TestTag3>());
        IsTrue (tags.HasAll(twoTags));
        IsTrue (tags.HasAny(twoTags));
        IsTrue (tags.HasAny(Tags.Get<TestTag2>()));

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
        
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        
        AreEqual("Tags: []", entity.Tags.ToString());
    }
    
    [Test]
    public static void Test_Tags_equality()
    {
        var tags  = Tags.Get<TestTag>();
        var tags1 = Tags.Get<TestTag>();
        var tags2 = Tags.Get<TestTag2>();
        
        IsTrue(tags == tags1);
        IsTrue(tags != tags2);
        IsTrue(tags.Equals(tags1));
        
        var e = Throws<NotImplementedException>(() => {
            _ = tags.Equals((object)tags1);
        });
        AreEqual("to prevent boxing", e!.Message);
        
        e = Throws<NotImplementedException>(() => {
            _ = tags.GetHashCode();
        });
        AreEqual("to prevent boxing", e!.Message);
    }
    
    [Test]
    public static void Test_Tags_constructor()
    {
        var schema  = EntityStore.GetEntitySchema();
        var tagType = schema.TagTypeByType[typeof(TestTag)];
        var tags    = new Tags(tagType);
        int count = 0;
        foreach (var tag in tags) {
            count++;
            AreSame(typeof(TestTag), tag.Type);
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_Tags_generic_IEnumerator_generic()
    {
        IEnumerable<TagType> tags = Tags.Get<TestTag>();
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
    public static void Test_Tags_generic_IEnumerator()
    {
        var tags = Tags.Get<TestTag>();

        IEnumerable enumerable = tags;
        var enumerator = enumerable.GetEnumerator();
        using var enumerator1 = enumerator as IDisposable;
        var count = 0;
        var schema = EntityStore.GetEntitySchema();
        while (enumerator.MoveNext()) {
            AreEqual(schema.GetTagType<TestTag>(), (TagType)enumerator.Current);
            count++;
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_Tags_Get()
    {
        var schema      = EntityStore.GetEntitySchema();
        var testTagType = schema.TagTypeByType[typeof(TestTag)];
        
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
        var store   = new EntityStore(PidType.RandomPids);
        var sig     = Signature.Get<Position>();

        var query1 = store.Query(sig).AllTags(Tags.Get<TestTag>());
        var query2 = store.Query(sig).AllTags(Tags.Get<TestTag, TestTag2>());
        
        AreEqual("Query: [Position, #TestTag]  Count: 0",             query1.ToString());
        AreEqual("Query: [Position, #TestTag, #TestTag2]  Count: 0",  query2.ToString());
    }
    
    [Test]
    public static void Test_Tags_Add_Remove()
    {
        var store       = new EntityStore(PidType.RandomPids);
        AreEqual(1,                                 store.Archetypes.Length);
        var entity      = store.CreateEntity();
        var testTag2    = Tags.Get<TestTag2>();
        
        var eventCount  = 0;
        Action<TagsChanged> handler     = args => {
            var str = args.ToString();
            switch (eventCount++) {
                case 0:     AreEqual(1,                             args.EntityId);
                            AreEqual("Tags: [#TestTag]",            args.Tags.          ToString());
                            AreEqual("Tags: []",                    args.OldTags.       ToString());
                            AreEqual("Tags: []",                    args.RemovedTags.   ToString());
                            AreEqual("Tags: [#TestTag]",            args.AddedTags.     ToString());
                            AreEqual("Tags: [#TestTag]",            args.ChangedTags.   ToString());
                            // Ensure entity is in new Archetype
                            AreEqual("[#TestTag]  entities: 1",     args.Entity.Archetype.ToString());
                            AreEqual("id: 1  [#TestTag]",           args.Entity.ToString());
                            AreSame (store,                         args.Store);
                            AreEqual("entity: 1 - event > Add Tags: [#TestTag]",            str);
                            return;
                
                case 1:     AreEqual("Tags: [#TestTag, #TestTag2]", args.Tags.          ToString());
                            AreEqual("Tags: [#TestTag]",            args.OldTags.       ToString());
                            AreEqual("Tags: []",                    args.RemovedTags.   ToString());
                            AreEqual("Tags: [#TestTag2]",           args.AddedTags.     ToString());
                            AreEqual("Tags: [#TestTag2]",           args.ChangedTags.   ToString());
                            AreEqual("entity: 1 - event > Add Tags: [#TestTag2]",           str);
                            return;
                
                case 2:     AreEqual("entity: 1 - event > Remove Tags: [#TestTag]",         str);   return;
                case 3:     AreEqual("entity: 1 - event > Remove Tags: [#TestTag2]",        str);   return;
                default:    Fail("unexpected event");                                               return;
            }
        };
        store.OnTagsChanged += handler;
       
        entity.AddTag<TestTag>();

        AreEqual("[#TestTag]  entities: 1",             entity.Archetype.ToString());
        AreEqual(1,                                     store.Count);
        AreEqual(2,                                     store.Archetypes.Length);
        
        // add same tag again
        entity.AddTag<TestTag>(); // no event sent
        AreEqual("[#TestTag]  entities: 1",             entity.Archetype.ToString());
        AreEqual(1,                                     store.Count);
        AreEqual(2,                                     store.Archetypes.Length);
        
        entity.AddTags(testTag2);
        AreEqual("[#TestTag, #TestTag2]  entities: 1",  entity.Archetype.ToString());
        AreEqual(1,                                     store.Count);
        AreEqual(3,                                     store.Archetypes.Length);
        
        entity.RemoveTag<TestTag>();
        AreEqual("[#TestTag2]  entities: 1",            entity.Archetype.ToString());
        AreEqual(1,                                     store.Count);
        AreEqual(4,                                     store.Archetypes.Length);
        
        entity.RemoveTags(testTag2);
        AreEqual("[]  entities: 1",                     entity.Archetype.ToString());
        AreEqual(1,                                     store.Count);
        AreEqual(4,                                     store.Archetypes.Length);
        
        // remove same tag again
        entity.RemoveTags(testTag2); // no event sent
        AreEqual("[]  entities: 1",                     entity.Archetype.ToString());
        AreEqual(1,                                     store.Count);
        AreEqual(4,                                     store.Archetypes.Length);
        
        store.OnTagsChanged -= handler;
        
        // Execute previous operations again. All required archetypes are now present
        const int count = 10; // 10_000_000 ~ #PC: 1.349 ms
        var start = Mem.GetAllocatedBytes();
        // each tags mutation causes a structural change
        for (int n = 0; n < count; n++) {
            entity.AddTag       <TestTag>();
            entity.AddTags      (testTag2);
            entity.RemoveTag    <TestTag>();
            entity.RemoveTags   (testTag2);
        }
        Mem.AssertNoAlloc(start);
        
        AreEqual(1,                                 store.Count);
        AreEqual(4,                                 store.Archetypes.Length);
        AreEqual(4, eventCount); // last assertion ensuring no events sent in perf test
    }
    
    /// <summary>Cover <see cref="EntityStoreBase.GetArchetypeWithTags"/></summary>
    [Test]
    public static void Test_Tags_cover_GetArchetypeWithTags() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        entity.AddComponent<Position>();
        entity.AddTag<TestTag>();
        
        var archetype = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag>());
        AreEqual(1, archetype.Count);
    }
    
    [Test]
    public static void Test_Tags_Query() {
        var store           = new EntityStore(PidType.RandomPids);
        var archTestTag     = store.GetArchetype(default, Tags.Get<TestTag>());
        var archTestTagAll  = store.GetArchetype(default, Tags.Get<TestTag, TestTag2>());
        AreEqual(3,                             store.Archetypes.Length);
        
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        entity1.AddTag<TestTag>();
        entity2.AddTag<TestTag>();
        entity2.AddTag<TestTag2>();
        AreEqual("[#TestTag]  entities: 1",             entity1.Archetype.ToString());
        AreEqual("[#TestTag, #TestTag2]  entities: 1",  entity2.Archetype.ToString());
        AreEqual(2,                                     store.Count);
        AreEqual(3,                                     store.Archetypes.Length);
        AreEqual(1,                                     archTestTag.Count);
        AreEqual(1,                                     archTestTagAll.Count);
        {
            var query  = store.Query().AllTags(Tags.Get<TestTag>());
            AreEqual("Query: [#TestTag]  Count: 2",     query.ToString());
            AreEqual("Entity[2]",                       query.Entities.ToString());
            int count   = 0;
            foreach (var entity in query.Entities) {
                switch (count) {
                    case 0: AreEqual(1, entity.Id); break;
                    case 1: AreEqual(2, entity.Id); break;
                }
                count++;
            }
            AreEqual(2, count);
        } {
            var query  = store.Query().AllTags(Tags.Get<TestTag2>());
            AreEqual("Query: [#TestTag2]  Count: 1", query.ToString());
            int count   = 0;
            foreach (var entity in query.Entities) {
                count++;
                AreEqual(2, entity.Id);
            }
            AreEqual(1, count);
        } { 
            var query = store.Query().AllTags(Tags.Get<TestTag, TestTag2>());
            AreEqual("Query: [#TestTag, #TestTag2]  Count: 1", query.ToString());
            int count   = 0;
            foreach (var entities in query.Entities) {
                count++;
                AreEqual(2, entities.Id);
            }
            AreEqual(1, count);
        }
    }
}

}

