using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS.Index {

public static class Test_Index_CommandBuffer
{
    // https://github.com/friflo/Friflo.Engine.ECS/issues/99
    [Test]
    public static void Test_Index_CommandBuffer_replace_value_new_component()
    {
        var store   = new EntityStore();
        var index   = store.ComponentIndex<IndexedInt, int>();
        store.CreateEntity(1);
        
        var ecb = store.GetCommandBuffer();
        ecb.AddComponent(1, new IndexedInt { value = 70 });
        ecb.AddComponent(1, new IndexedInt { value = 71 });   // replace previous value
        ecb.Playback();
        
        AreEqual(1,  index.Values.Count);
        AreEqual(1,  index[71].Count);
        AreEqual(0,  index[70].Count);
    }
    
    [Test]
    public static void Test_Index_CommandBuffer_replace_value_existing_component()
    {
        var store   = new EntityStore();
        var index   = store.ComponentIndex<IndexedInt, int>();
        var entity  = store.CreateEntity(1);
        entity.AddComponent(new IndexedInt { value = 69 });
        
        var ecb = store.GetCommandBuffer();
        ecb.AddComponent(1, new IndexedInt { value = 70 });
        ecb.AddComponent(1, new IndexedInt { value = 71 });
        ecb.Playback();
        
        AreEqual(1,  index.Values.Count);
        AreEqual(1,  index[71].Count);
        AreEqual(0,  index[69].Count);
        AreEqual(0,  index[70].Count);
        AreEqual(1,  index[71].Count);
    }
}

}
