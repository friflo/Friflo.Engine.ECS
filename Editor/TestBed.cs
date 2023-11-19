using System.Threading.Tasks;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor;

[Component("TestComponent")]
public struct  TestComponent : IComponent
{
    public string name;
}

public struct TestTag : IEntityTag { }


[Script("TestScript")]
public class TestScript : Script
{
    public string name;
}

public static class TestBed
{
    internal static async Task AddSampleEntities(GameDataSync sync)
    {
        var store   = sync.Store;
        var root    = store.StoreRoot;
        root.AddComponent(new Position(1, 1, 1));
        root.AddComponent(new EntityName("root"));
        var child   = CreateEntity(store, 2);
        child.AddComponent(new Position(2, 2, 2));

        root.AddChild(child);
        root.AddChild(CreateEntity(store, 3));
        root.AddChild(CreateEntity(store, 4));
        root.AddChild(CreateEntity(store, 5));
        root.AddChild(CreateEntity(store, 6));
        root.AddChild(CreateEntity(store, 7));
        
        await sync.StoreGameEntitiesAsync();
    }
    
    private static GameEntity CreateEntity(GameEntityStore store, int id)
    {
        var entity = store.CreateEntity();
        entity.AddComponent(new EntityName("child-" + id));
        return entity;
    }
}