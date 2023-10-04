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
        NotNull(store.CreateQuery<Position>());
        NotNull(store.CreateQuery<Position,Rotation>());
    }

    [Test]
    public static void Test_BitArray()
    {
        var v1 = Vector256.Create(0);
        var v2 = Vector256.Create(1);
        _ = v1 | v2;
    }
}

