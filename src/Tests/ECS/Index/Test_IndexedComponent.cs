using System.IO;
using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {

public static class Test_IndexedComponent
{
    private const string JSON_withIndexedComponent =
@"[{
    ""id"": 1,
    ""components"": {
        ""IndexedInt"": {""value"":42}
    }
}";
    
private const string JSON_withoutIndexedComponent =
@"[{
    ""id"": 1,
    ""components"": {
    }
}";
    
    [Test]
    public static void Test_IndexedComponent_serialize_add()
    {
        var store   = new EntityStore();
        var index   = store.ComponentIndex<IndexedInt, int>();
        store.CreateEntity(1);
    
        var serializer = new EntitySerializer();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(JSON_withIndexedComponent));
        serializer.ReadIntoStore(store, readStream);
        AreEqual(1, index[42].Count);
        AreEqual(1, index.Values.Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_serialize_update()
    {
        var store = new EntityStore();
        var index = store.ComponentIndex<IndexedInt, int>();
        Entity entity = store.CreateEntity(1);
        entity.AddComponent(new IndexedInt { value = 10 });
        AreEqual(1, index[10].Count);
        
        var serializer = new EntitySerializer();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(JSON_withIndexedComponent));
        serializer.ReadIntoStore(store, readStream);
        AreEqual(1, index[42].Count);
        AreEqual(0, index[10].Count);
        AreEqual(1, index.Values.Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_serialize_remove()
    {
        var store  = new EntityStore();
        var index  = store.ComponentIndex<IndexedInt, int>();
        var entity = store.CreateEntity(1);
        entity.AddComponent(new IndexedInt { value = 10 });
        AreEqual(1, index[10].Count);
        // entity.AddComponent(new MyComponent1 { a = 10 });
        
        var serializer = new EntitySerializer();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(JSON_withoutIndexedComponent));
        serializer.ReadIntoStore(store, readStream);
        AreEqual(0, index[10].Count);
        AreEqual(0, index.Values.Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_CreateEntity()
    {
        var store = new EntityStore();
        var index = store.ComponentIndex<IndexedInt, int>();
        store.CreateEntity(new IndexedInt { value = 22 });
        AreEqual(1, index[22].Count);
        
        store.CreateEntity(new MyComponent1(), new IndexedInt { value = 22 });
        AreEqual(2, index[22].Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityExtension_Add()
    {
        var store = new EntityStore();
        var index = store.ComponentIndex<IndexedInt, int>();
        var entity = store.CreateEntity();
        
        entity.Add(new IndexedInt { value = 30 });
        AreEqual(1, index[30].Count);
        AreEqual(1, index.Values.Count);
        AreEqual("Count: 1", index.ToString());
        
        entity.Add(new IndexedInt { value = 31 });
        AreEqual(1, index.Values.Count);
        AreEqual(0, index[30].Count);
        AreEqual(1, index[31].Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityExtension_Set()
    {
        var store = new EntityStore();
        var index = store.ComponentIndex<IndexedInt, int>();
        var entity = store.CreateEntity(new IndexedInt { value = 40 });
        
        entity.Set(new IndexedInt { value = 41 });
        AreEqual(1, index[41].Count);
        AreEqual(1, index.Values.Count);
        
        entity.Set(new IndexedInt { value = 42 });
        AreEqual(1, index.Values.Count);
        AreEqual(0, index[41].Count);
        AreEqual(1, index[42].Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityExtension_Remove()
    {
        var store = new EntityStore();
        var index = store.ComponentIndex<IndexedInt, int>();
        var entity = store.CreateEntity();
        entity.AddComponent(new IndexedInt { value = 40 });
        AreEqual(1, index[40].Count);
        AreEqual(1, index.Values.Count);
        
        entity.Remove<IndexedInt>();
        AreEqual(0, index[40].Count);
        AreEqual(0, index.Values.Count);
        
        entity.Remove<IndexedInt>();
        AreEqual(0, index[40].Count);
        AreEqual(0, index.Values.Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityBatch_ApplyTo()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var index   = store.ComponentIndex<IndexedInt, int>();
        var batch   = new EntityBatch();
        batch.Add   (new IndexedInt { value = 50 });
        
        var entity1 = store.CreateEntity(1);
        batch.ApplyTo(entity1);
        AreEqual(1, index[50].Count);
        AreEqual(1, index.Values.Count);
        
        batch   = new EntityBatch();
        batch.Add   (new IndexedInt { value = 51 });
        batch.ApplyTo(entity1);
        AreEqual(1, index[51].Count);
        AreEqual(0, index[50].Count);
        AreEqual(1, index.Values.Count);
        
        batch   = new EntityBatch();
        batch.Remove<IndexedInt>();
        batch.ApplyTo(entity1);
        AreEqual(0, index[51].Count);
        AreEqual(0, index.Values.Count);
        
        batch.ApplyTo(entity1);
    }
    
    [Test]
    public static void Test_IndexedComponent_CreateEntityBatch()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var index = store.ComponentIndex<IndexedInt, int>();
        var batch = new CreateEntityBatch(store);
        batch.Add(new Position())
            .Add(new IndexedInt { value = 60 });
        
        batch.CreateEntity();
        AreEqual(1, index[60].Count);
        AreEqual(1, index.Values.Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_CommandBuffer()
    {
        var store   = new EntityStore();
        var index   = store.ComponentIndex<IndexedInt, int>();
        store.CreateEntity(1);
        

        var ecb = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        ecb.AddComponent(1, new IndexedInt { value = 70 });
        ecb.Playback();
        
        AreEqual(1, index[70].Count);
        AreEqual(1, index.Values.Count);
        
        ecb.AddComponent(1, new IndexedInt { value = 71 });
        ecb.Playback();
        
        AreEqual(0, index[70].Count);
        AreEqual(1, index[71].Count);
        AreEqual(1, index.Values.Count);
        
        ecb.AddComponent(1, new IndexedInt { value = 72 });
        ecb.Playback();
        
        AreEqual(0, index[71].Count);
        AreEqual(1, index[72].Count);
        AreEqual(1, index.Values.Count);
        
        ecb.RemoveComponent<IndexedInt>(1);
        ecb.RemoveComponent<IndexedInt>(1);
        ecb.Playback();
        
        AreEqual(0, index[72].Count);
        AreEqual(0, index.Values.Count);
        
        ecb.RemoveComponent<IndexedInt>(1);
        ecb.Playback();
        
        AreEqual(0, index[72].Count);
        AreEqual(0, index.Values.Count);
    }

}

}