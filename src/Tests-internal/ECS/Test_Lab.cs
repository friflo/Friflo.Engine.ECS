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


internal struct IndexedName : IIndexedComponent<string> {
    public      string  Value => name;
    internal    string  name;
}

internal struct IndexedInt : IIndexedComponent<int> {
    public      int     Value => value;
    internal    int     value;
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
        
        entities[0].AddComponent(new IndexedName   { name   = "find-me"});
        entities[1].AddComponent(new IndexedInt    { value  = 123      });
    //  entities[1].AddComponent(new AttackComponent { target = target }); // todo throws NotImplementedException : to avoid excessive boxing. ...
        
        var query1  = world.Query<Position, IndexedName>().  Has<IndexedName,   string>("find-me");
        var query2  = world.Query<Position, IndexedInt>().   Has<IndexedInt,    int>   (123);
        var query3  = world.Query<Position, AttackComponent>().Has<AttackComponent, Entity>(target);
        
        int countNames = 0;
        query1.ForEachEntity((ref Position _, ref IndexedName indexedName, Entity _) => {
            countNames++;
            AreEqual("find-me", indexedName.name);
        });
        AreEqual(1, countNames);
        
        int intCount = 0;
        query2.ForEachEntity((ref Position _, ref IndexedInt indexedInt, Entity _) => {
            intCount++;
            AreEqual(123, indexedInt.value);
        });
        AreEqual(1, intCount);
        
        query3.ForEachEntity((ref Position _, ref AttackComponent attack, Entity _) => {
            AreEqual(target, attack.target);
        });
    }
}

}
