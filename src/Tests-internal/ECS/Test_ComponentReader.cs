using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;

namespace Internal.ECS {

// ReSharper disable once InconsistentNaming
public static class Test_ComponentReader
{
    /// <summary>cover <see cref="RawComponent.ToString"/></summary>
    [Test]
    public static void Test_ComponentReader_RawComponent() {
        var unresolved      = EntityStore.GetEntitySchema().unresolvedType;
        var rawKey          = new RawKey("test", unresolved);
        var rawComponent    = new RawComponent(rawKey, 0, 0);
        Assert.AreEqual("test - Unresolved", rawComponent.ToString());
    }
}

}