using Friflo.Engine.ECS;
using NUnit.Framework;


// ReSharper disable once InconsistentNaming
namespace Internal.ECS {

public static class Test_Query
{
    [Test]
    public static void Test_Query_ChunkEntitiesDebugView()
    {
        var store = new EntityStore();
        store.CreateEntity(1).AddComponent<Position>();
        store.CreateEntity(2).AddComponent<Position>();
        
        var query = store.Query<Position>();

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
        var store = new EntityStore();
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
        var store = new EntityStore();
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
}

}