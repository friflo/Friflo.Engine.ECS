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
        
        // AreSame(tag2, Tags.Get<TestTag2, TestTag>()); todo
    }
}

