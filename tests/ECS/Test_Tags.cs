using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

internal struct TestTag : IEntityTag { }

internal struct TestTag2 : IEntityTag { }

public static class Test_Tags
{
    [Test]
    public static void Test_SignatureTypes()
    {
        var schema          = EntityStore.GetComponentSchema();
        var testTagType     = schema.TagTypeByType[typeof(TestTag)];
        var testTagType2    = schema.TagTypeByType[typeof(TestTag2)];
        
        var tag1    = Tags.Get<TestTag>();
        AreEqual("[#TestTag]", tag1.ToString());
        NotNull(tag1);
        int count1 = 0;
        tag1.ForEach((tagType) => {
            AreSame(testTagType, tagType);
            count1++;
        });
        AreEqual(1, count1);
        
        var count2 = 0;
        var tag2 = Tags.Get<TestTag, TestTag2>();
        AreEqual("[#TestTag, #TestTag2]", tag2.ToString());
        NotNull(tag2);
        tag2.ForEach((tagType) => {
            count2++;
        });
        AreEqual(2, count2);
        
        AreEqual(tag2, Tags.Get<TestTag2, TestTag>());
    }
    
    [Test]
    public static void Test_tagged_Query() {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        var sig1 = Signature.Get<Position>();
        var sig2 = Signature.Get<Position, Rotation>();
        var sig3 = Signature.Get<Position, Rotation, Scale3>();
        var sig4 = Signature.Get<Position, Rotation, Scale3, MyComponent1>();
        var sig5 = Signature.Get<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        //

        var query2 =    store.Query(sig2, Tags.Get<TestTag, TestTag2>());
    }

}

