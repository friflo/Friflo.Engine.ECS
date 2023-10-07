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
        var tag1 = Tags.Get<TestTag>();
        NotNull(tag1);
        
        var tag2 = Tags.Get<TestTag, TestTag2>();
        NotNull(tag2);
        
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

