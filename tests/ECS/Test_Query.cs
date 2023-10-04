using System;
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
    public static void Test_ArchetypeMask()
    {
        {
            var mask = new ArchetypeMask(Array.Empty<int>());
            AreEqual("<0, 0, 0, 0>", mask.ToString());
        } {
            var mask = new ArchetypeMask(new [] { 0 });
            AreEqual("<1, 0, 0, 0>", mask.ToString());
        } {
            var mask = new ArchetypeMask(new [] { 0, 64, 128, 192 });
            AreEqual("<1, 1, 1, 1>", mask.ToString());
        }  {
            var mask = new ArchetypeMask(new [] { 63, 127, 191, 255 });
            AreEqual("<-9223372036854775808, -9223372036854775808, -9223372036854775808, -9223372036854775808>", mask.ToString());
        }
    }
}

