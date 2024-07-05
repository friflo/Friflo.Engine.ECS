using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {
    
public static class Test_IncomingLinks
{
    [Test]
    public static void Test_IncomingLinks_All()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        entity2.AddComponent(new LinkComponent   { entity = entity1, data  = 1 });
        entity2.AddComponent(new AttackComponent { target = entity1, data  = 2 });
        entity3.AddComponent(new LinkComponent   { entity = entity1, data  = 3 });
        entity3.AddComponent(new AttackComponent { target = entity1, data  = 4 });
        entity2.AddRelation (new AttackRelation  { target = entity1, speed = 5 });
        entity3.AddRelation (new AttackRelation  { target = entity1, speed = 6 });
        
        var links = entity1.GetAllIncomingLinks();
        AreEqual("IncomingLinks[6]  Target entity: 1",  links.ToString());
        AreEqual(6,                                     links.Count);
        AreEqual("Entity: 2  [AttackComponent]",links[0].ToString());
        AreEqual(2,                             links[0].Entity.Id);
        AreEqual(2,         ((AttackComponent)  links[0].Component).data);
            
        AreEqual(3,                             links[1].Entity.Id);
        AreEqual(4,         ((AttackComponent)  links[1].Component).data);
            
        AreEqual(2,                             links[2].Entity.Id);
        AreEqual(1,         ((LinkComponent)    links[2].Component).data);
            
        AreEqual(3,                             links[3].Entity.Id);
        AreEqual(3,         ((LinkComponent)    links[3].Component).data);
            
        AreEqual(2,                             links[4].Entity.Id);
        AreEqual(5,         ((AttackRelation)   links[4].Component).speed);
            
        AreEqual(3,                             links[5].Entity.Id);
        AreEqual(6,         ((AttackRelation)   links[5].Component).speed);
    }
    
    [Test]
    public static void Test_IncomingLinks_LinkComponent()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        var target4 = store.CreateEntity(4);
        var target5 = store.CreateEntity(5);
       
        entity1.AddComponent(new LinkComponent { entity = target4, data = 100 });
        entity2.AddComponent(new LinkComponent { entity = target5, data = 101  });
        entity3.AddComponent(new LinkComponent { entity = target5, data = 102  });

        var refs4    = target4.GetIncomingLinks<LinkComponent>();
        AreEqual("IncomingLinks<LinkComponent>[1]  Target entity: 4", refs4.ToString());
        AreEqual("{ 1 }",       refs4.Debug());
        AreSame (store,         refs4.Store);
        AreEqual(4,             refs4.Target.Id);
        AreEqual(1,             refs4.Count);
        AreEqual(1,             refs4.Entities.Count);
        AreEqual("Entity: 1",   refs4[0].ToString());
        AreEqual(1,             refs4[0].Entity.Id);
        AreEqual(100,           refs4[0].Component.data);
        
        var refs5    = target5.GetIncomingLinks<LinkComponent>();
        AreEqual("{ 2, 3 }",    refs5.Debug());
        AreEqual(5,             refs5.Target.Id);
        AreEqual("Entity: 2",   refs5[0].ToString());
        AreEqual(2,             refs5[0].Entity.Id);
        AreEqual(101,           refs5[0].Component.data);
        AreEqual("Entity: 3",   refs5[1].ToString());
        AreEqual(3,             refs5[1].Entity.Id);
        AreEqual(102,           refs5[1].Component.data);

        int count = 0;
        foreach (var reference in refs5) {
            switch (count++) {
                case 0: AreEqual(101, reference.Component.data);    break;
                case 1: AreEqual(102, reference.Component.data);    break;
            }
        }
        AreEqual(2, count);
    }
    
    [Test]
    public static void Test_IncomingLinks_LinkRelation()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        var target10 = store.CreateEntity(10);
        var target11 = store.CreateEntity(11);
        
        entity1.AddRelation(new AttackRelation { target = target10, speed = 100 });
        entity2.AddRelation(new AttackRelation { target = target10, speed = 101  });
        entity3.AddRelation(new AttackRelation { target = target10, speed = 102  });
        entity3.AddRelation(new AttackRelation { target = target11, speed = 103  });

        var refs10   = target10.GetIncomingLinks<AttackRelation>();
        AreEqual("{ 1, 2, 3 }",     refs10.Debug());
        AreEqual(3,                 refs10.Count);
        AreEqual(10,                refs10.Target.Id);
        AreEqual(1,                 refs10[0].Entity.Id);
        AreEqual(100,               refs10[0].Component.speed);
        AreEqual(2,                 refs10[1].Entity.Id);
        AreEqual(101,               refs10[1].Component.speed);
        AreEqual(3,                 refs10[2].Entity.Id);
        AreEqual(102,               refs10[2].Component.speed);
            
        var refs11 = target11.GetIncomingLinks<AttackRelation>();
        AreEqual("{ 3 }",       refs11.Debug());
        AreEqual(1,             refs11.Count);
        AreEqual(11,            refs11.Target.Id);
        AreEqual(3,             refs11[0].Entity.Id);
        AreEqual(103,           refs11[0].Component.speed);
    }
    
}

}