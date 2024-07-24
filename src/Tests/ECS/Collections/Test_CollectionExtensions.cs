using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Collections {

public static class Test_CollectionExtensions
{
    [Test]
    public static void Test_CollectionExtensions_Chunk(){
        var chunk = new Chunk<Position>();
        AreEqual("{ }", chunk.Debug());
    }
}

}
