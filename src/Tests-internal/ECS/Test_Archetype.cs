using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_Archetype
{
    [Test]
    public static void Test_Archetype_Key()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var posType     = store.GetArchetype(ComponentTypes.Get<Position>());
        var posRotType  = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        
        AreEqual(1,                             posType.ComponentTypes.Count);
        AreEqual(2,                             posRotType.ComponentTypes.Count);
        AreEqual("Key: [Position]",             posType.key.ToString());
        AreEqual("Key: [Position, Rotation]",   posRotType.key.ToString());
    }
    
    [Test]
    public static void Test_Archetype_ComponentAttribute()
    {
        _ = new ComponentKeyAttribute("abc");
        _ = new TagNameAttribute("foo");
        
        var type = typeof(Test_Archetype);
        var handle = type.Handle();
        var expect = type.TypeHandle.Value.ToInt64();
        AreEqual(expect, handle);
    }
    
    [Test]
    public static void Test_Archetypes_ToString()
    {
        var archetypes = new Archetypes(new Archetype[1], 1);
        AreEqual("Archetype[1]", archetypes.ToString());
    }
    
    /// <summary>
    /// cover <see cref="StructHeap{T}.ToString"/>
    /// </summary>
    [Test]
    public static void Test_Archetype_StructHeap_ToString()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var entity      = store.CreateEntity();
        entity.AddComponent<Position>();
        var posType     = store.GetArchetype(ComponentTypes.Get<Position>());
        StructHeap heap = posType.Heaps()[0];
        AreEqual("StructHeap<Position>  Capacity: 512", heap.ToString());
    }
    
    [Test]
    public static void Test_Archetype_Tags_Query()
    {
        var store           = new EntityStore(PidType.RandomPids);
        var archTestTag     = store.GetArchetype(default, Tags.Get<TestTag>());
        var archTestTagAll  = store.GetArchetype(default, Tags.Get<TestTag, TestTag2>());
        AreEqual(3,                             store.Archetypes.Length);
        AreEqual("Key: [#TestTag]",             archTestTag.key.ToString());
        AreEqual("Key: [#TestTag, #TestTag2]",  archTestTagAll.key.ToString());
    }
    
    [Test]
    public static void Test_Archetype_Entities_query_ToString()
    {
        var store   = new EntityStore(PidType.RandomPids);
        
        var arch    = store.GetArchetype(ComponentTypes.Get<Position>());
        AreEqual("Archetype: [Position]  Count: 0", arch.Entities.query.ToString());
        
        arch    = store.GetArchetype(ComponentTypes.Get<Position>(), Tags.Get<TestTag>());
        AreEqual("Archetype: [Position, #TestTag]  Count: 0", arch.Entities.query.ToString());
    }
    
    [Test]
    public static void Test_Archetype_EntityStore_Entities_query_ToString()
    {
        var store   = new EntityStore(PidType.RandomPids);
        AreEqual("Query: []  Count: 0", store.Entities.query.ToString());
    }
    
    
    [Test]
    public static void Test_Archetype_SignatureIndexes()
    {
        /* Throws<IndexOutOfRangeException>(() => {
            _ = new SignatureIndexes (11);
        }); */
        
        var schema  = EntityStore.GetEntitySchema();
        var posType = schema.GetComponentType<Position>();
        
        var indexes = new SignatureIndexes(posType.StructIndex);
        AreEqual("SignatureIndexes: [Position]", indexes.ToString());
    }
    
    [Test]
    public static void Test_Archetype_SignatureIndexes_coverage()
    {
        object obj = new SignatureIndexes();
        obj.SetInternalField(nameof(SignatureIndexes.length), 11);
        var indexes = (SignatureIndexes)obj;
        AreEqual(-1, indexes.GetStructIndex(10));
    }
    
    [Test]
    public static void Test_Archetype_ComponentTypes_coverage()
    {
        object obj = new SignatureIndexes();
        obj.SetInternalField(nameof(SignatureIndexes.length), 0);
        var indexes = (SignatureIndexes)obj;
        Throws<IndexOutOfRangeException>(() => {
            _ = new ComponentTypes(indexes);
        });
    }
    
    
    [Test]
    public static void Test_Archetype_Equality()
    {
        var store       = new EntityStore();
        var arch1       = store.GetArchetype(ComponentTypes.Get<Position>());
        var arch2       = store.GetArchetype(ComponentTypes.Get<Rotation>());
        var comparer    = ArchetypeKeyEqualityComparer.Instance;
        
        IsFalse(comparer.Equals(arch1.key, arch2.key));
    }
}

}

