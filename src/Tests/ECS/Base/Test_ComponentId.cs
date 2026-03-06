using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {
    
// -------- example implementing a switch statement on component types --------
struct Melee  : IComponent { }
struct Ranged : IComponent { }
struct Tank   : IComponent { }
    
public enum CombatType
{
                                    Undefined,
    [MapComponent(typeof(Melee))]   Melee,
    [MapComponent(typeof(Ranged))]  Ranged,
    [MapComponent(typeof(Tank))]    Tank,
}


public static class Test_TypeId
{
    [Test]
    public static void Test_TypeId_switch()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.Add(new Position(), new Melee(), new Ranged(), new Tank());
        
        bool foundUndefined = false;
        bool foundMelee     = false;
        bool foundRanged    = false;
        bool foundTank      = false;

        foreach (var component in entity.Components)
        {
            CombatType combatId = component.Type.AsEnum<CombatType>();
            switch (combatId) {
                case CombatType.Melee:
                    foundMelee = true;
                    _ = entity.GetComponent<Melee>();
                    break;
                case CombatType.Ranged:
                    foundRanged = true;
                    _ = entity.GetComponent<Ranged>();
                    break;
                case CombatType.Tank:
                    foundTank = true;
                    _ = entity.GetComponent<Tank>();
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
}

}
