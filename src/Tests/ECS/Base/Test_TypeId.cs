using System;
using System.Reflection;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {
    
/// <summary>
/// Utility class that enables to implement switch statements for specific component types.<br/>
/// E.g. when iterating <see cref="Entity.Components"/> or <see cref="Archetype.ComponentTypes"/>.
/// </summary>
/// <remarks> 
/// This is done by creating and enum type and assign its values to specific component types.
/// <code>
/// public enum CombatType
/// {
///                                 Undefined,
///     [TypeId(typeof(Melee))]     Melee,
///     [TypeId(typeof(Ranged))]    Ranged,
///     [TypeId(typeof(Tank))]      Tank,
/// }
///
/// // switch statement on enum CombatType
/// foreach (var component in entity.Components)
/// {
///     CombatType combatType = TypeId&lt;CombatType>.Of(component.Type);
///     switch (combatType) {
///         case CombatType.Melee:  var ranged = entity.GetComponent&lt;Melee>(); ...
///         case CombatType.Ranged:  ...
///         case CombatType.Tank:   ...
///         default:                ...
///     }
/// }
/// </code>
/// This helper class may become part of the ECS library.
/// </remarks>
public static class TypeId<TEnum> where TEnum : Enum
{
    private static readonly TEnum[] typeIds = CreateTypeIds();
    
    /// <summary> return type id in O(1). Simply an index lookup </summary>
    public static TEnum Of(ComponentType from) => typeIds[from.StructIndex];
    
    private static TEnum[] CreateTypeIds()
    {
        var schema = EntityStore.GetEntitySchema();
        var componentTypes = schema.Components;
        var ids = new TEnum[componentTypes.Length];
        var enumValues = Enum.GetValues(typeof(TEnum));
        foreach (var value in enumValues)
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()!)[0];
            var attribute = (TypeIdAttribute)memberInfo.GetCustomAttribute(typeof(TypeIdAttribute), false);
            if (attribute == null) {
                continue;
            }
            var componentType = schema.ComponentTypeByType[attribute.type];
            ids[componentType.StructIndex] = (TEnum)value;
        }
        return ids;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class TypeIdAttribute : Attribute {
    public readonly Type type;
    public TypeIdAttribute (Type type) => this.type = type;
}




// -------- example implementing a switch statement for specific component types --------
struct Melee  : IComponent { }
struct Ranged : IComponent { }
struct Tank   : IComponent { }
    
public enum CombatType
{
                                Undefined,
    [TypeId(typeof(Melee))]     Melee,
    [TypeId(typeof(Ranged))]    Ranged,
    [TypeId(typeof(Tank))]      Tank,
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
            CombatType combatType = TypeId<CombatType>.Of(component.Type);
            switch (combatType) {
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
