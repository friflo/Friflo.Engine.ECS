using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_ArchetypeStructs
{
    [Test]
    public static void Test_SignatureIndexes_coverage()
    {
        object obj = new SignatureIndexes();
        obj.SetInternalField(nameof(SignatureIndexes.length), 6);
        var indexes = (SignatureIndexes)obj;
        AreEqual(-1, indexes.GetStructIndex(5));
    }
    
    [Test]
    public static void Test_ArchetypeKey()
    {
        var store       = new GameEntityStore();
        var posType     = store.GetArchetype(Signature.Get<Position>());
        var posRotType  = store.GetArchetype(Signature.Get<Position, Rotation>());
        
        AreEqual(1,                             posType.Structs.Count);
        AreEqual(2,                             posRotType.Structs.Count);
        AreEqual("Key: [Position]",             posType.key.ToString());
        AreEqual("Key: [Position, Rotation]",   posRotType.key.ToString());
    }
    
    [Test]
    public static void Test_StructComponentAttribute()
    {
        _ = new StructComponentAttribute("abc");
        _ = new ClassComponentAttribute("xyz");
        
        var type = typeof(Test_ArchetypeStructs);
        var handle = type.Handle();
        var expect = type.TypeHandle.Value.ToInt64();
        AreEqual(expect, handle);
    }
    
    [Test]
    public static void Test_StructHeap_ToString()
    {
        var store       = new GameEntityStore();
        var entity      = store.CreateEntity();
        entity.AddComponent<Position>();
        var posType     = store.GetArchetype(Signature.Get<Position>());
        StructHeap heap = posType.Heaps[0];
#if DEBUG
        AreEqual("[Position] heap - Count: 1", heap.ToString());
#else
        AreEqual("[Position] heap", heap.ToString());
#endif
        var genericHeap = (StructHeap<Position>)heap;
        AreEqual("used", genericHeap.chunks[0].ToString());
    }
}

