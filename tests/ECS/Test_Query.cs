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
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        var posQuery    = store.Query<Position>();
        var posRotQuery = store.Query<Position, Rotation>();
        AreEqual(0, posQuery.Archetypes.Length);
        AreEqual(0, posRotQuery.Archetypes.Length);
        
        entity.AddComponent<Position>();
        AreEqual(1, posQuery.Archetypes.Length);
        AreEqual(0, posRotQuery.Archetypes.Length);
        
        entity.AddComponent<Rotation>();
        AreEqual(2, posQuery.Archetypes.Length);
        AreEqual(1, posRotQuery.Archetypes.Length);
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

