using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch;

#pragma warning disable CS0618 // Type or member is obsolete
public static class Test_Deprecated
{
    [Test]
    public static void Test_Deprecated_properties()
    {
        var store = new EntityStore();
        var query = store.Query();
        AreEqual(0, query.EntityCount); // replaced by Count
    }
}