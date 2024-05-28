using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.Client {

public static class Test_Client
{
    
    /// <summary>cover <see cref="ReadResult.ToString"/></summary>
    [Test]
    public static void Test_Client_ReadEntitiesResult_ToString()
    {
        var result = new ReadResult(2, "test error");
        AreEqual("entityCount: 2 error: test error", result.ToString());
    }
}

}