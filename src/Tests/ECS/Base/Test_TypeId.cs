using System;
using System.Reflection;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {
    
/// <summary>
/// Utility class used get an enum id mapped to component type.<br/>
/// This enables the use of switch statements on <see cref="ComponentType"/>'s.<br/>
/// E.g. when iterating <see cref="Entity.Components"/> or <see cref="Archetype.ComponentTypes"/>.<br/>
/// Or to handle specific <see cref="ComponentType"/>'s in event handlers like <see cref="ComponentChanged"/>.<br/>
/// It also improves project overview by grouping a domain of component types to an enum.
/// </summary>
/// <remarks>
/// Usage:<br/>
/// Declare an <c>enum</c> and map component types to enum ids with <c>[MapComponent()]</c>.
/// <code>
/// public enum CombatType
/// {
///     Undefined = 0, // 0 => unmapped component types switch to default case 
///     [MapComponent(typeof(Melee))]   Melee,
///     [MapComponent(typeof(Ranged))]  Ranged,
///     [MapComponent(typeof(Tank))]    Tank,
/// }
///
/// // switch statement on enum CombatType
/// foreach (var component in entity.Components)
/// {
///     CombatType combatType = ComponentId&lt;CombatType>.Of(component.Type);
///     switch (combatType) {
///         case CombatType.Melee:  var ranged = entity.GetComponent&lt;Melee>(); ...  break;
///         case CombatType.Ranged: ...  break;
///         case CombatType.Tank:   ...  break;
///         default:                ...  break;
///     }
/// }
/// </code>
/// This helper class may become part of the ECS library.
/// </remarks>
public static class ComponentId<TEnum> where TEnum : Enum
{
    private static readonly TEnum[] idMap = CreateTypeIds();
    
    /// <summary>
    /// Returns the enum id mapped to a component type with a <c>[MapComponent()]</c> attribute.
    /// </summary>
    /// <remarks> Executes in O(1). Simply an index lookup. </remarks>
    public static TEnum Of(ComponentType from) => idMap[from.StructIndex];
    
    private static TEnum[] CreateTypeIds()
    {
        var schema = EntityStore.GetEntitySchema();
        var componentTypes = schema.Components;
        var ids = new TEnum[componentTypes.Length];
        var enumValues = Enum.GetValues(typeof(TEnum));
        foreach (var value in enumValues)
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()!)[0];
            var attribute = (MapComponentAttribute)memberInfo.GetCustomAttribute(typeof(MapComponentAttribute), false);
            if (attribute == null) {
                continue;
            }
            var componentType = schema.ComponentTypeByType[attribute.type];
            ids[componentType.StructIndex] = (TEnum)value;
        }
        return ids;
    }
}

/// <summary>
/// Maps a component type to a custom enum id.<br/>
/// This enables the use of switch statements for <see cref="ComponentType"/>'s.<br/>
/// Usage see: <see cref="ComponentId{TEnum}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class MapComponentAttribute : Attribute {
    public readonly Type type;
    public MapComponentAttribute (Type type) => this.type = type;
}




// -------- example implementing a switch statement for specific component types --------
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
            CombatType combatType = ComponentId<CombatType>.Of(component.Type);
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
