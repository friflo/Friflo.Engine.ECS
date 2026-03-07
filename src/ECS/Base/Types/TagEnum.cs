using System.Reflection;
using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Maps a tag type to an enum id.<br/>
/// This enables the use of switch statements on tag types using <see cref="TagType.AsEnum{TEnum}"/>.<br/>
/// E.g. when iterating <see cref="Entity.Tags"/> or <see cref="Archetype.Tags"/>.<br/>
/// Or to handle specific <see cref="TagType"/>'s in event handlers like <see cref="TagsChanged"/>.<br/>
/// It also improves project overview by grouping a domain of tag types to an enum.
/// </summary>
/// <remarks>
/// Usage:<br/>
/// Declare an <c>enum</c> and map tag types to enum ids with <c>[MapTag()]</c>.
/// <code>
/// public enum CombatTags
/// {
///     Undefined = 0, // 0 => unmapped tag types switch to default case 
///     [MapTag(typeof(MeleeTag))]      Melee,
///     [MapTag(typeof(RangedTag))]     Ranged,
///     [MapTag(typeof(TankTag))]       Tank,
/// }
/// </code>
/// <code>
/// // switch statement on enum CombatTags
/// foreach (var tag in entity.Tags)
/// {
///     var combatId = tag.AsEnum&lt;CombatTags>();
///     switch (combatId) {
///         case CombatTags.Melee:  ...  break;
///         case CombatTags.Ranged: ...  break;
///         case CombatTags.Tank:   ...  break;
///         default:                ...  break;
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class MapTagAttribute : Attribute {
    public readonly Type type;
    public MapTagAttribute (Type type) => this.type = type;
}

/// <summary>
/// Maps a tag type to an enum id.<br/>
/// This enables the use of switch statements on tag types using <see cref="TagType.AsEnum{TEnum}"/>.<br/>
/// E.g. when iterating <see cref="Entity.Tags"/> or <see cref="Archetype.Tags"/>.<br/>
/// Or to handle specific <see cref="TagType"/>'s in event handlers like <see cref="TagsChanged"/>.<br/>
/// It also improves project overview by grouping a domain of tag types to an enum.
/// </summary>
/// <remarks>
/// Usage:<br/>
/// Declare an <c>enum</c> and map tag types to enum ids with <c>[MapTag()]</c>.
/// <code>
/// public enum CombatTags
/// {
///     Undefined = 0, // 0 => unmapped tag types switch to default case
///     [MapTag&lt;MeleeTag>]   Melee,
///     [MapTag&lt;RangedTag>]  Ranged,
///     [MapTag&lt;TankTag>]    Tank,
/// }
/// </code>
/// <code>
/// // switch statement on enum CombatTags
/// foreach (var tag in entity.Tags)
/// {
///     var combatId = tag.AsEnum&lt;CombatTags>();
///     switch (combatId) {
///         case CombatTags.Melee:  ...  break;
///         case CombatTags.Ranged: ...  break;
///         case CombatTags.Tank:   ...  break;
///         default:                ...  break;
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class MapTagAttribute<TTag> : Attribute where TTag : struct, ITag { }


internal static class TagEnum<TEnum> where TEnum : struct, Enum
{
    internal static readonly TEnum[] IdMap = CreateIdMap();
    
    private static TEnum[] CreateIdMap()
    {
        var schema = EntityStore.GetEntitySchema();
        var tagTypes = schema.Tags;
        var ids = new TEnum[tagTypes.Length];
        var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
        foreach (var value in enumValues)
        {
            var type = GetAttributeType(value);
            if (type == null) {
                continue;
            }
            var tagType = schema.TagTypeByType[type];
            ids[tagType.TagIndex] = value;
        }
        return ids;
    }
    
    private static Type GetAttributeType(TEnum value)
    {
        var memberInfo = typeof(TEnum).GetMember(value.ToString()!)[0];
        var attribute = (MapTagAttribute)memberInfo.GetCustomAttribute(typeof(MapTagAttribute), false);
        if (attribute != null) {
            return attribute.type;
        }
        // generic attributes requires C# 11 or higher
        var attributeGeneric = memberInfo.GetCustomAttribute(typeof(MapTagAttribute<>), false);
        if (attributeGeneric != null) {
            return attributeGeneric.GetType().GenericTypeArguments[0];
        }
        return null;
    }
}
