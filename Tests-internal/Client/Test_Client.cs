using System;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.Client;

public static class Test_Client
{
    
    /// <summary>cover <see cref="ReadSceneResult.ToString"/></summary>
    [Test]
    public static void Test_Client_ReadSceneResult_ToString()
    {
        var result = new ReadSceneResult(2, "test error");
        AreEqual("entityCount: 2 error: test error", result.ToString());
    }
}