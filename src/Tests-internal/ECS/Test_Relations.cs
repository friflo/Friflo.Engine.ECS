using Friflo.Engine.ECS;
using NUnit.Framework;
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
        entity.AddComponent(new StringRelation { value = "test" });
        
        var relations = store.relationsMap[StructInfo<StringRelation>.Index];
        AreEqual("relation count: 1", relations.ToString());
    }
}

}
