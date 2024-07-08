using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {
    
public static class Test_EntityLinks
{
    [Test]
    public static void Test_EntityLinks_AllIncomingLinks()
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
        AreEqual("{ 2, 3, 2, 3, 2, 3 }",        links.Debug());
        AreEqual(6,                             links.Count);
        AreEqual(6, entity1.CountAllIncomingLinks());
        AreEqual("EntityLinks[6]",              links.ToString());
        AreEqual(6,                             links.Count);
        
        AreEqual("Entity: 2 -> Target: 1  [AttackComponent]",links[0].ToString());
        AreEqual(2,                             links[0].Entity.Id);
        AreEqual(1,                             links[0].Target.Id);
        AreEqual(2,         ((AttackComponent)  links[0].Component).data);
            
        AreEqual(3,                             links[1].Entity.Id);
        AreEqual(1,                             links[0].Target.Id);
        AreEqual(4,         ((AttackComponent)  links[1].Component).data);
            
        AreEqual(2,                             links[2].Entity.Id);
        AreEqual(1,         ((LinkComponent)    links[2].Component).data);
            
        AreEqual(3,                             links[3].Entity.Id);
        AreEqual(3,         ((LinkComponent)    links[3].Component).data);
            
        AreEqual(2,                             links[4].Entity.Id);
        AreEqual(5,         ((AttackRelation)   links[4].Component).speed);
            
        AreEqual(3,                             links[5].Entity.Id);
        AreEqual(6,         ((AttackRelation)   links[5].Component).speed);
        
        entity1.DeleteEntity();
        AreEqual(0, entity2.Components.Count);
        AreEqual(0, entity3.Components.Count);

        int count = 0;
        foreach (var _ in links) {
            count++;
        }
        AreEqual(6, count);
        {
            IEnumerable enumerable = links;
            var enumerator = enumerable.GetEnumerator();
            using var enumerator1 = enumerator as IDisposable;
            enumerator.Reset();
            count = 0;
            while (enumerator.MoveNext()) {
                count++;
                _ = enumerator.Current;
            }
            AreEqual(6, count);
        } {
            count = 0;
            IEnumerable<EntityLink> enumerable = links;
            foreach (var _ in enumerable) {
                count++;
            }
            AreEqual(6, count);
        }
    }
    
    [Test]
    public static void Test_EntityLinks_IncomingLinkComponent()
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
        AreEqual("EntityLinks<LinkComponent>[1]", refs4.ToString());
        AreEqual("{ 1 }",       refs4.Debug());
        AreSame (store,         refs4.Store);
        AreEqual(1,             refs4.Count);
        AreEqual(1,             refs4.Entities.Count);
        AreEqual("Entity: 1 -> Target: 4",   refs4[0].ToString());
        AreEqual(1,             refs4[0].Entity.Id);
        AreEqual(100,           refs4[0].Component.data);
        _ =                     ref refs4[0].Component; // ensure component can be accessed by ref
        
        var refs5    = target5.GetIncomingLinks<LinkComponent>();
        AreEqual("{ 2, 3 }",    refs5.Debug());
        AreEqual("Entity: 2 -> Target: 5",   refs5[0].ToString());
        AreEqual(2,             refs5[0].Entity.Id);
        AreEqual(101,           refs5[0].Component.data);
        AreEqual("Entity: 3 -> Target: 5",   refs5[1].ToString());
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
        
        IEnumerable enumerable = refs5;
        var enumerator = enumerable.GetEnumerator();
        using var enumerator1 = enumerator as IDisposable;
        enumerator.Reset();
        while (enumerator.MoveNext()) {
            _ = enumerator.Current;
        }
    }
    
    [Test]
    public static void Test_EntityLinks_IncomingLinkRelation()
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
        AreEqual(1,                 refs10[0].Entity.Id);
        AreEqual(100,               refs10[0].Component.speed);
        AreEqual(10,                refs10[0].Target.Id);
        AreEqual(2,                 refs10[1].Entity.Id);
        AreEqual(101,               refs10[1].Component.speed);
        AreEqual(10,                refs10[1].Target.Id);
        AreEqual(3,                 refs10[2].Entity.Id);
        AreEqual(102,               refs10[2].Component.speed);
        AreEqual(10,                refs10[2].Target.Id);
            
        var refs11 = target11.GetIncomingLinks<AttackRelation>();
        AreEqual("{ 3 }",       refs11.Debug());
        AreEqual(1,             refs11.Count);
        AreEqual(3,             refs11[0].Entity.Id);
        AreEqual(103,           refs11[0].Component.speed);
    }
    
    [Test]
    public static void Test_EntityLinks_AllOutgoingLinks()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        AreEqual(0, entity1.CountAllOutgoingLinks());
        
        entity1.AddComponent<Position>();
        entity1.AddComponent(new IndexedInt());
        entity1.AddRelation(new IntRelation());
        AreEqual(0, entity1.CountAllOutgoingLinks());
        
        entity1.AddComponent(new AttackComponent());
        AreEqual(1, entity1.CountAllOutgoingLinks());
        
        entity1.AddRelation(new AttackRelation { target = entity2 });
        AreEqual(2, entity1.CountAllOutgoingLinks());
        
        entity1.AddRelation(new AttackRelation { target = entity3 });
        AreEqual(3, entity1.CountAllOutgoingLinks());
    }
    
    [Test]
    public static void Test_EntityLinks_screenshot()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        entity2.AddComponent(new AttackComponent { target = entity1 });
        entity3.AddRelation (new AttackRelation  { target = entity1 });
        
        EntityLinks_screenshot(entity1);
    }
    
    // make screenshot with scale 350% - on 4K monitor. Size[px]: ~ 1890 x 1110
    private static void EntityLinks_screenshot(Entity entity) {
        _ = entity;
    }
    
}

}