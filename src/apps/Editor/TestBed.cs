using Friflo.Engine.ECS;

// ReSharper disable RedundantTypeDeclarationBody
namespace Friflo.Editor;

public struct  TestComponent : IComponent
{
    public string   name;
    public Position start;
    public Scale3   scale;
}

public struct Tag1 : ITag { }
public struct Tag2 : ITag { }
public struct Tag3 : ITag { }
public struct Tag4 : ITag { }

public class Script1 : Script
{
    public  string      name;
    public  Position    spawn;
    public  Position    target;
}

public class Script2 : Script
{
    public  int         maxHealth;
    public  Position    center;
    public  Scale3      scale;
}

public static class TestBed
{
    internal static void AddSampleEntities(EntityStore store)
    {
        var root    = store.StoreRoot;
        /*
        root.AddComponent(new Transform { m11 = 1, m12 = 2, m13 = 3 });
        root.AddComponent(new EntityName("root"));
        root.AddTag<Tag1>();
        root.AddTag<Tag2>();
        root.AddScript(new Script1 { name = "Peter", spawn = new Position(3, 3, 3), target = new Position(4, 4, 4)});
        root.AddScript(new Script2 { maxHealth = 42, center = new Position(10, 10, 10)});
        */
        var child2   = CreateEntity(store, 2);
        child2.AddComponent(new Transform { m11 = 4, m12 = 5, m13 = 6 });
        child2.AddTag<Tag1>();
        child2.AddTag<Tag2>();
        child2.AddTag<Tag3>();
        child2.AddScript(new Script1 { name = "Mary" });

        root.AddChild(child2);
        var child3 = CreateEntity(store, 3);
        child3.AddTag<Tag1>();
        child3.AddTag<Tag2>();
        child3.AddTag<Tag3>();
        child3.AddTag<Tag4>();
        root.AddChild(child3);
        root.AddChild(CreateEntity(store, 4));
        root.AddChild(CreateEntity(store, 5));
        root.AddChild(CreateEntity(store, 6));
        var entity7 = CreateEntity(store, 7);
        entity7.AddComponent(new TestComponent { name = "abc", scale = new Scale3(3,3,3), start = new Position(4,4,4)});
        root.AddChild(entity7);
        root.AddChild(store.CreateEntity());
        root.AddChild(store.CreateEntity());
        CreateManyEntities(root, "10.000",       new [] { 100, 100 });
        // CreateManyEntities(root, "1.000.000",    new [] { 100, 100, 100 });
    }
    
    private static void CreateManyEntities(Entity root, string name, int[] counts)
    {
        var many = root.Store.CreateEntity();
        many.AddComponent(new EntityName(name));
        AddManyEntities(many, counts, 0);
        root.AddChild(many);
    }
    
    private static void AddManyEntities(Entity entity, int[] counts, int depth)
    {
        if (depth >= counts.Length) {
            return;
        }
        var count   = counts[depth];
        var store   = entity.Store;
        for (int n = 0; n < count; n++) {
            var child = store.CreateEntity();
            entity.AddChild(child);
            AddManyEntities(child, counts, depth + 1);
        }
    }
    
    private static Entity CreateEntity(EntityStore store, int id)
    {
        var entity = store.CreateEntity();
        entity.AddComponent(new EntityName("child - " + id));
        return entity;
    }
}