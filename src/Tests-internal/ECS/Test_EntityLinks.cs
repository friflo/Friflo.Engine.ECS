using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Internal.ECS {
    
public static class Test_EntityLinks
{
    [Test]
    public static void Test_EntityLinks_All()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        var links = entity1.GetAllIncomingLinks();
        AreEqual("{ }", links.Debug());
        
        entity2.AddComponent(new LinkComponent   { entity = entity1, data  = 1 });
        entity2.AddComponent(new AttackComponent { target = entity1, data  = 2 });
        entity3.AddComponent(new LinkComponent   { entity = entity1, data  = 3 });
        entity3.AddComponent(new AttackComponent { target = entity1, data  = 4 });
        entity2.AddRelation (new AttackRelation  { target = entity1, speed = 5 });
        entity3.AddRelation (new AttackRelation  { target = entity1, speed = 6 });
        
        AreEqual("links incoming: 6 outgoing: 0", entity1.Info.ToString());
        AreEqual("links incoming: 0 outgoing: 3", entity2.Info.ToString());
        AreEqual("links incoming: 0 outgoing: 3", entity3.Info.ToString());
        links = entity1.Info.IncomingLinks;

        var debugView = new EntityLinksDebugView(links);
        AreEqual(6, debugView.links.Length);
    }
    
    [Test]
    public static void Test_EntityLinks_LinkComponent()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        var target4 = store.CreateEntity(4);
        var target5 = store.CreateEntity(5);
        var refs4    = target4.GetIncomingLinks<LinkComponent>();
        AreEqual("{ }", refs4.Debug());
       
        entity1.AddComponent(new LinkComponent { entity = target4, data = 100 });
        entity2.AddComponent(new LinkComponent { entity = target5, data = 101  });
        entity3.AddComponent(new LinkComponent { entity = target5, data = 102  });

        refs4    = target4.GetIncomingLinks<LinkComponent>();

        var debugView = new EntityLinksDebugView<LinkComponent>(refs4);
        AreEqual(1, debugView.Entities.Length);
    }
}

}