using Friflo.Fliox.Engine.Client;
using NUnit.Framework;

namespace Internal.ECS;

// ReSharper disable once InconsistentNaming
public static class Test_ComponentReader
{
    [Test]
    public static void Test_ComponentReader_RawComponent() {
        var rawComponent = new RawComponent { key = "test" };
        Assert.AreEqual("test", rawComponent.ToString());
    }
    
    [Test]
    public static void Test_ComponentReader_DataNode() {
        var dataNode = new DataNode { pid = 1234 };
        Assert.AreEqual("pid: 1234", dataNode.ToString());
    }
}