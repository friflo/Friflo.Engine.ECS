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
        var store = new EntityStore();
        store.CreateEntity(1);
    
        var serializer = new EntitySerializer();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(JSON_withIndexedComponent));
        serializer.ReadIntoStore(store, readStream);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(42).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_serialize_update()
    {
        var store = new EntityStore();
        Entity entity = store.CreateEntity(1);
        entity.AddComponent(new IndexedInt { value = 10 });
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(10).Count);
        
        var serializer = new EntitySerializer();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(JSON_withIndexedComponent));
        serializer.ReadIntoStore(store, readStream);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(42).Count);
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(10).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_serialize_remove()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity(1);
        entity.AddComponent(new IndexedInt { value = 10 });
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(10).Count);
        // entity.AddComponent(new MyComponent1 { a = 10 });
        
        var serializer = new EntitySerializer();
        var readStream = new MemoryStream(Encoding.UTF8.GetBytes(JSON_withoutIndexedComponent));
        serializer.ReadIntoStore(store, readStream);
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(10).Count);
        AreEqual(0, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_CreateEntity()
    {
        var store = new EntityStore();
        store.CreateEntity(new IndexedInt { value = 22 });
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(22).Count);
        
        store.CreateEntity(new MyComponent1(), new IndexedInt { value = 22 });
        AreEqual(2, store.GetEntitiesWithComponentValue<IndexedInt,int>(22).Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityExtension_Add()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        
        entity.Add(new IndexedInt { value = 30 });
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(30).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        entity.Add(new IndexedInt { value = 31 });
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(30).Count);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(31).Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityExtension_Set()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity(new IndexedInt { value = 40 });
        
        entity.Set(new IndexedInt { value = 41 });
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(41).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        entity.Set(new IndexedInt { value = 42 });
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(41).Count);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(42).Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityExtension_Remove()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        entity.AddComponent(new IndexedInt { value = 40 });
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(40).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        entity.Remove<IndexedInt>();
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(40).Count);
        AreEqual(0, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        entity.Remove<IndexedInt>();
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(40).Count);
        AreEqual(0, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_EntityBatch_ApplyTo()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var batch   = new EntityBatch();
        batch.Add   (new IndexedInt { value = 50 });
        
        var entity1 = store.CreateEntity(1);
        batch.ApplyTo(entity1);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(50).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        batch   = new EntityBatch();
        batch.Add   (new IndexedInt { value = 51 });
        batch.ApplyTo(entity1);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(51).Count);
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(50).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        batch   = new EntityBatch();
        batch.Remove<IndexedInt>();
        batch.ApplyTo(entity1);
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(51).Count);
        AreEqual(0, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        batch.ApplyTo(entity1);
    }
    
    [Test]
    public static void Test_IndexedComponent_CreateEntityBatch()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        var batch = new CreateEntityBatch(store);
        batch.Add(new Position())
            .Add(new IndexedInt { value = 60 });
        
        batch.CreateEntity();
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(60).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
    }
    
    [Test]
    public static void Test_IndexedComponent_CommandBuffer()
    {
        var store   = new EntityStore();
        store.CreateEntity(1);
        

        var ecb = store.GetCommandBuffer();
        ecb.ReuseBuffer = true;
        ecb.AddComponent(1, new IndexedInt { value = 70 });
        ecb.Playback();
        
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(70).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        ecb.AddComponent(1, new IndexedInt { value = 71 });
        ecb.Playback();
        
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(70).Count);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(71).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        ecb.AddComponent(1, new IndexedInt { value = 72 });
        ecb.Playback();
        
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(71).Count);
        AreEqual(1, store.GetEntitiesWithComponentValue<IndexedInt,int>(72).Count);
        AreEqual(1, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        ecb.RemoveComponent<IndexedInt>(1);
        ecb.RemoveComponent<IndexedInt>(1);
        ecb.Playback();
        
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(72).Count);
        AreEqual(0, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
        
        ecb.RemoveComponent<IndexedInt>(1);
        ecb.Playback();
        
        AreEqual(0, store.GetEntitiesWithComponentValue<IndexedInt,int>(72).Count);
        AreEqual(0, store.GetAllIndexedComponentValues<IndexedInt,int>().Count);
    }

}

}