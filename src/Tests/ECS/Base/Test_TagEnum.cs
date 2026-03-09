using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {
    
// -------- example implementing a switch statement on tag types --------
struct MeleeTag  : ITag { }
struct RangedTag : ITag { }
struct TankTag   : ITag { }
    
public enum CombatTags
{
                                 Undefined,
    [MapTag(typeof(MeleeTag))]   Melee,
    [MapTag(typeof(RangedTag))]  Ranged,
    [MapTag(typeof(TankTag))]    Tank,
}

public enum MapTagError
{
    Undefined,
    [MapTag(typeof(TestTag))]   TestTag,
    [MapTag(typeof(TestTag))]   MapException,
}


public static class Test_TagEnum
{
    [Test]
    public static void Test_TagEnum_switch()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddTags(Tags.Get<MeleeTag, RangedTag, TankTag, TestTag>());
        
        bool foundUndefined = false;
        bool foundMelee     = false;
        bool foundRanged    = false;
        bool foundTank      = false;

        foreach (var tag in entity.Tags)
        {
            var tagId = tag.AsEnum<CombatTags>();
            switch (tagId) {
                case CombatTags.Melee:
                    foundMelee = true;
                    break;
                case CombatTags.Ranged:
                    foundRanged = true;
                    break;
                case CombatTags.Tank:
                    foundTank = true;
                    break;
                default:
                    foundUndefined = true;
                    break;
            }
        }
        IsTrue(foundUndefined);
        IsTrue(foundMelee);
        IsTrue(foundRanged);
        IsTrue(foundTank);
    }
    
    [Test]
    public static void Test_TagEnum_Map_exception()
    {
        var schema = EntityStore.GetEntitySchema();
        var type = schema.TagTypeByType[typeof(TestTag)];
        
        var e = Throws<TypeInitializationException>(() => {
            type.AsEnum<MapTagError>();
        });
        AreEqual("Map error: [MapTag<TestTag>] MapTagError.MapException - Already mapped to: MapTagError.TestTag", e.InnerException.Message);
    }
}

}
