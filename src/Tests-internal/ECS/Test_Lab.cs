using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

internal struct AttackComponent : IIndexedComponent<Entity> {
    public      Entity  Value => target;
    internal    Entity  target;
}


internal struct NameComponent : IIndexedComponent<string> {
    public      string  Value => name;
    internal    string  name;
}


public static class Test_Lab
{
    [Test]
    public static void Test_Lab_EntityLink()
    {
        var world = new EntityStore();
        var targets = new EntityList(world);
        for (int n = 0; n < 10; n++) {
            targets.Add(world.CreateEntity());
        }
        var entities = new List<Entity>();
        for (int n = 0; n < 10; n++) {
            var entity = world.CreateEntity(new Position(n, 0, 0), new AttackComponent{target = targets[n]});
            entities.Add(entity);
        }
        Entity target = targets[0];
        
        entities[0].AddComponent(new NameComponent   { name   = "find-me"});
        entities[1].AddComponent(new AttackComponent { target = target });
        
        var query1  = world.Query<Position, NameComponent>().  Has<NameComponent,   string>("find-me");
        var query2  = world.Query<Position, AttackComponent>().Has<AttackComponent, Entity>(target);
        
        query1.ForEachEntity((ref Position _, ref NameComponent name, Entity _) => {
            AreEqual("target", name.name);
        });
        query2.ForEachEntity((ref Position _, ref AttackComponent attack, Entity _) => {
            AreEqual(target, attack.target);
        });
    }
}

}
