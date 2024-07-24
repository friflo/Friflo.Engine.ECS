using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;


// ReSharper disable once InconsistentNaming
namespace Internal.ECS {

public static class Test_Query
{
    [Test]
    public static void Test_Query_ChunkEntitiesDebugView()
    {
        var store = new EntityStore(PidType.RandomPids);
        store.CreateEntity(1).AddComponent<Position>();
        store.CreateEntity(2).AddComponent<Position>();
        
        var query = store.Query<Position>();
        Assert.AreEqual("Components: [Position]", query.ComponentTypes.ToString());
        

        foreach (var (_, entities) in query.Chunks) {
            var debugView       = new ChunkEntitiesDebugView(entities);
            var debugEntities   = debugView.Entities;
            Assert.AreEqual(2, debugEntities.Length);
            Assert.AreEqual(1, debugEntities[0].Id);
            Assert.AreEqual(2, debugEntities[1].Id);
        }
    }
    
    [Test]
    public static void Test_Query_QueryEntitiesDebugView()
    {
        var store = new EntityStore(PidType.RandomPids);
        store.CreateEntity(1).AddComponent<Position>();
        store.CreateEntity(2).AddComponent<Position>();
        
        var query       = store.Query<Position>();
        var debugView   = new QueryEntitiesDebugView(query.Entities);
        
        var debugEntities = debugView.Entities;
        Assert.AreEqual(2, debugEntities.Length);
        Assert.AreEqual(1, debugEntities[0].Id);
        Assert.AreEqual(2, debugEntities[1].Id);
    }
    
    [Test]
    public static void Test_Query_ChunkDebugView()
    {
        var store = new EntityStore(PidType.RandomPids);
        store.CreateEntity(1).AddComponent<Position>();
        store.CreateEntity(2).AddComponent<Position>();
        
        var query = store.Query<Position>();
        var count = 0;
        foreach (var chunk in query.Chunks) {
            count++;
            Assert.AreEqual("Position[2]", chunk.Chunk1.ToString());
            var debugView   = new ChunkDebugView<Position>(chunk.Chunk1);
            var positions   = debugView.Components;
            Assert.AreEqual(2, positions.Length);
        }
        Assert.AreEqual(1, count);
    }
    
    [Test]
    public static void Test_Query_ChunkEntitiesEnumerator()
    {
        var store = new EntityStore();
        var type = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 1; n <= 10; n++) {
            type.CreateEntity(n);
        }
        var entities = new ChunkEntities(type, 10, 0);
        int id = 1;
        foreach (var entity in entities) {
            Assert.AreEqual(id++, entity.Id);
        }
        id = 6;
        var section = new ChunkEntities(entities, 5, 5, 0);
        foreach (var entity in section) {
            Assert.AreEqual(id++, entity.Id);
        }  
    }
    
    [Test]
    public static void Test_Query_ChunkEntities_index_operator()
    {
        var store = new EntityStore();
        var type = store.GetArchetype(ComponentTypes.Get<MyComponent1>());
        for (int n = 1; n <= 10; n++) {
            type.CreateEntity(n);
        }
        var entities = new ChunkEntities(type, 10, 0);
        int id = 1;
        for (int n = 0; n < 10; n++) {
            Assert.AreEqual(id, entities[n]);
            Assert.AreEqual(id, entities.EntityAt(n).Id);
            id++;
        }
        id = 6;
        var section = new ChunkEntities(entities, 5, 5, 0);
        for (int n = 0; n < 5; n++) {
            Assert.AreEqual(id, section[n]);
            Assert.AreEqual(id, section.EntityAt(n).Id);
            id++;
        }
    }
}

}