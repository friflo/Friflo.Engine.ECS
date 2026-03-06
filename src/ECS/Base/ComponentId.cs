using System.Reflection;
using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Utility class used to get the enum id mapped to a component type.<br/>
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
/// </code>
/// <code>
/// // switch statement on enum CombatType
/// foreach (var component in entity.Components)
/// {
///     var combatType = ComponentId&lt;CombatType>.Of(component.Type);
///     switch (combatType) {
///         case CombatType.Melee:  var ranged = entity.GetComponent&lt;Melee>(); ...  break;
///         case CombatType.Ranged: ...  break;
///         case CombatType.Tank:   ...  break;
///         default:                ...  break;
///     }
/// }
/// </code>
/// </remarks>
public static class ComponentId<TEnum> where TEnum : struct, Enum
{
    private static readonly TEnum[] IdMap = CreateIdMap();
    
    /// <summary>
    /// Returns the enum id mapped to a component type with a <c>[MapComponent()]</c> attribute.<br/>
    /// Returns 0 if the passed component type is not mapped.
    /// </summary>
    /// <remarks> Executes in O(1). Simply an array index lookup. </remarks>
    public static TEnum Of(ComponentType componentType) => IdMap[componentType.StructIndex];
    
    private static TEnum[] CreateIdMap()
    {
        var schema = EntityStore.GetEntitySchema();
        var componentTypes = schema.Components;
        var ids = new TEnum[componentTypes.Length];
        var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
        foreach (var value in enumValues)
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()!)[0];
            var attribute = (MapComponentAttribute)memberInfo.GetCustomAttribute(typeof(MapComponentAttribute), false);
            if (attribute == null) {
                continue;
            }
            var componentType = schema.ComponentTypeByType[attribute.type];
            ids[componentType.StructIndex] = value;
        }
        return ids;
    }
}

/// <summary>
/// Maps a component type to an enum id.<br/>
/// This enables the use of switch statements for <see cref="ComponentType"/>'s.<br/>
/// Usage see: <see cref="ComponentId{TEnum}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class MapComponentAttribute : Attribute {
    public readonly Type type;
    public MapComponentAttribute (Type type) => this.type = type;
}