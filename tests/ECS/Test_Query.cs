using System.Runtime.Intrinsics;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Query
{
    [Test]
    public static void Test_Create_Query()
    {
        IsTrue(true);
        var store = new EntityStore();
        var posQuery = store.Query<Position>();
        NotNull (posQuery);
        AreEqual(0, posQuery.Archetypes.Length);
        
        var posRotQuery = store.Query<Position,Rotation>();
        NotNull (posRotQuery);
        AreEqual(0, posRotQuery.Archetypes.Length);
    }

    [Test]
    public static void Test_BitArray()
    {
        var v1 = Vector256.Create(0);
        var v2 = Vector256.Create(1);
        _ = v1 | v2;
    }
}

