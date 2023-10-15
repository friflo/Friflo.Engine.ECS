using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.ECS.Base;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS;


public static class Test_Tags
{
    [Test]
    public static void Test_Tags_Query()
    {
        var store           = new GameEntityStore();
        var archTestTag     = store.GetArchetype(Tags.Get<TestTag>());
        var archTestTagAll  = store.GetArchetype(Tags.Get<TestTag, TestTag2>());
        AreEqual(3,                             store.Archetypes.Length);
        AreEqual("Key: [#TestTag]",             archTestTag.key.ToString());
        AreEqual("Key: [#TestTag, #TestTag2]",  archTestTagAll.key.ToString());
    }
}

