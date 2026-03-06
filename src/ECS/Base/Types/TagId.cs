using System.Reflection;
using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Utility class used to get the enum id mapped to a tag type.<br/>
/// This enables the use of switch statements on <see cref="TagType"/>'s.<br/>
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
///     var combatId = TagId&lt;CombatTags>.Of(tag);
///     switch (combatId) {
///         case CombatTags.Melee:  ...  break;
///         case CombatTags.Ranged: ...  break;
///         case CombatTags.Tank:   ...  break;
///         default:                ...  break;
///     }
/// }
/// </code>
/// </remarks>
public static class TagId<TEnum> where TEnum : struct, Enum
{
    internal static readonly TEnum[] IdMap = CreateIdMap();
    
    /// <summary>
    /// Returns the enum id mapped to a tag type with a <c>[MapTag()]</c> attribute.<br/>
    /// Returns 0 if the passed tag type is not mapped.
    /// </summary>
    /// <remarks> Executes in O(1). Simply an array index lookup. </remarks>
    public static TEnum Of(TagType tagType) => IdMap[tagType.TagIndex];
    
    private static TEnum[] CreateIdMap()
    {
        var schema = EntityStore.GetEntitySchema();
        var tagTypes = schema.Tags;
        var ids = new TEnum[tagTypes.Length];
        var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
        foreach (var value in enumValues)
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()!)[0];
            var attribute = (MapTagAttribute)memberInfo.GetCustomAttribute(typeof(MapTagAttribute), false);
            if (attribute == null) {
                continue;
            }
            var tagType = schema.TagTypeByType[attribute.type];
            ids[tagType.TagIndex] = value;
        }
        return ids;
    }
}

/// <summary>
/// Maps a tag type to an enum id.<br/>
/// This enables the use of switch statements for <see cref="TagType"/>'s.<br/>
/// Usage see: <see cref="TagId{TEnum}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class MapTagAttribute : Attribute {
    public readonly Type type;
    public MapTagAttribute (Type type) => this.type = type;
}