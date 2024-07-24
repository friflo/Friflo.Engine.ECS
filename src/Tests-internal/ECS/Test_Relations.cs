using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Relations;
using NUnit.Framework;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

internal struct StringRelation : IRelationComponent<string>
{
    public  string  value;
    public  string  GetRelationKey()    => value;

    public override string ToString()   => value;
}


public static class Test_Relations
{
    [Test]
    public static void Test_Relations_EntityRelations()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddRelation(new StringRelation { value = "test" });
        
        var relations = store.extension.relationsMap[StructInfo<StringRelation>.Index];
        AreEqual("relation count: 1", relations.ToString());
    }
    
    [Test]
    public static void Test_Relations_exception()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddRelation(new StringRelation { value = "test" });
        
        var relations = store.extension.relationsMap[StructInfo<StringRelation>.Index];
        var e = Throws<InvalidOperationException>(() => {
            relations.RemoveLinksWithTarget(0);    
        });
        AreEqual("type: EntityRelations`2", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            relations.GetEntityRelation<StringRelation>(1, 2);
        });
        AreEqual("type: EntityRelations`2", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            relations.AddIncomingRelations(0, null);
        });
        AreEqual("type: EntityRelations`2", e!.Message);
    }
    
    [Test]
    public static void Test_Relations_LinkRelationUtils()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        entity1.AddRelation(new AttackRelation { target = entity2 });
        
        var relations = (EntityRelationLinks<AttackRelation>)store.extension.relationsMap[StructInfo<AttackRelation>.Index];
        LinkRelationUtils.AddComponentValue(1, 2, relations);
        AreEqual(1, relations.Count);
        
        LinkRelationUtils.RemoveComponentValue(42, 2, relations);
        AreEqual(1, relations.Count);
    }
}

}
