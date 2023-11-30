using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Archetype
{
    [Test]
    public static void Test_Archetype_Key()
    {
        var store       = new EntityStore();
        var posType     = store.GetArchetype(Signature.Get<Position>());
        var posRotType  = store.GetArchetype(Signature.Get<Position, Rotation>());
        
        AreEqual(1,                             posType.ComponentTypes.Count);
        AreEqual(2,                             posRotType.ComponentTypes.Count);
        AreEqual("Key: [Position]",             posType.key.ToString());
        AreEqual("Key: [Position, Rotation]",   posRotType.key.ToString());
    }
    
    [Test]
    public static void Test_Archetype_ComponentAttribute()
    {
        _ = new ComponentAttribute("abc");
        _ = new ScriptAttribute("xyz");
        
        var type = typeof(Test_Archetype);
        var handle = type.Handle();
        var expect = type.TypeHandle.Value.ToInt64();
        AreEqual(expect, handle);
    }
    
    /// <summary>
    /// cover <see cref="StructHeap{T}.ToString"/>
    /// cover <see cref="StructChunk{T}.ToString"/>
    /// </summary>
    [Test]
    public static void Test_Archetype_StructHeap_ToString()
    {
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        entity.AddComponent<Position>();
        var posType     = store.GetArchetype(Signature.Get<Position>());
        StructHeap heap = posType.Heaps[0];
#if DEBUG
        AreEqual("[Position] chunks - Count: 1, Length: 1, EntityCount: 1", heap.ToString());
#else
        AreEqual("[Position] chunks - Count: 1, Length: 1", heap.ToString());
#endif
        var genericHeap = (StructHeap<Position>)heap;
        AreEqual("used", genericHeap.chunks[0].ToString());
    }
    
    [Test]
    public static void Test_Archetype_Tags_Query()
    {
        var store           = new EntityStore();
        var archTestTag     = store.GetArchetype(Tags.Get<TestTag>());
        var archTestTagAll  = store.GetArchetype(Tags.Get<TestTag, TestTag2>());
        AreEqual(3,                             store.Archetypes.Length);
        AreEqual("Key: [#TestTag]",             archTestTag.key.ToString());
        AreEqual("Key: [#TestTag, #TestTag2]",  archTestTagAll.key.ToString());
    }
    
    
    [Test]
    public static void Test_Archetype_SignatureIndexes()
    {
        Throws<IndexOutOfRangeException>(() => {
            _ = new SignatureIndexes (6);
        });
        
        var indexes = new SignatureIndexes(0);
        Throws<IndexOutOfRangeException>(() => {
            indexes.GetStructIndex(0);
        });
        var schema  = EntityStore.GetEntitySchema();
        var posType = schema.GetComponentType<Position>();
        
        indexes = new SignatureIndexes(1, posType.structIndex);
        AreEqual("StructIndexes: [Position]", indexes.ToString());
    }
    
    [Test]
    public static void Test_Archetype_SignatureIndexes_coverage()
    {
        object obj = new SignatureIndexes();
        obj.SetInternalField(nameof(SignatureIndexes.length), 6);
        var indexes = (SignatureIndexes)obj;
        AreEqual(-1, indexes.GetStructIndex(5));
    }
}

