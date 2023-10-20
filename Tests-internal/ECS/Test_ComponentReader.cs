using Friflo.Fliox.Engine.ECS.Sync;
using NUnit.Framework;

namespace Internal.ECS;

// ReSharper disable once InconsistentNaming
public static class Test_ComponentReader
{
    /// <summary>cover <see cref="RawComponent.ToString"/></summary>
    [Test]
    public static void Test_ComponentReader_RawComponent() {
        var rawComponent = new RawComponent("test", 0, 0);
        Assert.AreEqual("test", rawComponent.ToString());
    }
    
    [Test]
    public static void Test_ComponentReader_DataNode() {
        var dataNode = new DataNode { pid = 1234 };
        Assert.AreEqual("pid: 1234", dataNode.ToString());
    }
}