using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS.Index;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {
    
public static class Test_IncomingLinks
{
    [Test]
    public static void Test_IncomingLinks_Entity_IncomingLinks()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        entity2.AddComponent(new LinkComponent   { entity = entity1, data = 1 });
        entity2.AddComponent(new AttackComponent { target = entity1, data = 2 });
        entity3.AddComponent(new LinkComponent   { entity = entity1, data = 3 });
        entity3.AddComponent(new AttackComponent { target = entity1, data = 4 });
    //  entity2.AddRelation (new AttackRelation  { target = entity1 });
        
        var links = entity1.IncomingLinks;
        AreEqual("IncomingLinks[4]  Target: 1", links.ToString());
        AreEqual(4,             links.Count);
        AreEqual("Entity: 2  [AttackComponent]",  links[0].ToString());
        AreEqual(2,                         links[0].Entity.Id);
        AreEqual(2,     ((AttackComponent)  links[0].Component).data);
        
        AreEqual(3,                         links[1].Entity.Id);
        AreEqual(4,     ((AttackComponent)  links[1].Component).data);
        
        AreEqual(2,                         links[2].Entity.Id);
        AreEqual(1,     ((LinkComponent)    links[2].Component).data);
        
        AreEqual(3,                         links[3].Entity.Id);
        AreEqual(3,     ((LinkComponent)    links[3].Component).data);
    }
    
}

}