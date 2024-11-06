using System.IO;
using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Index;
using static NUnit.Framework.Assert;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public static class Test_SerializeIndexedComponent
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
    public static void Test_SerializeIndexedComponent_add()
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
    public static void Test_SerializeIndexedComponent_update()
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
    public static void Test_SerializeIndexedComponent_remove()
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
    public static void Test_IndexedComponent_Add()
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
    public static void Test_IndexedComponent_Set()
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
    public static void Test_IndexedComponent_Remove()
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
}

}