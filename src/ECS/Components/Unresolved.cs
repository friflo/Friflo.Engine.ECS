// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// An <see cref="Unresolved"/> component is added to a <see cref="GameEntity"/> by <see cref="EntityConverter"/>.<see cref="EntityConverter.DataToGameEntity"/> if:<br/>
/// <list type="bullet">
///   <item>
///     A component in <see cref="DataEntity"/>.<see cref="DataEntity.components"/> cannot be resolved to an <see cref="IComponent"/> or <see cref="Behavior"/> type. 
///   </item>
///   <item>
///     A tag in <see cref="DataEntity"/>.<see cref="DataEntity.tags"/>  cannot be resolved to an <see cref="IEntityTag"/> type.
///   </item>
/// </list>
/// </summary>
/// <remarks>
/// The <see cref="Unresolved"/> component enables conversion of a <see cref="DataEntity"/> to <see cref="GameEntity"/> and vice versa<br/>
/// with components or tags that cannot be resolved to <see cref="IEntityTag"/>, <see cref="IComponent"/> and <see cref="Behavior"/> types.<br/>
/// <br/>
/// Having support <see cref="Unresolved"/> component or tag types:
/// <list type="bullet">
///   <item>
///     Ensures the ability to use a scene containing unresolved types without being blocked by a missing fix.
///   </item>
///   <item>
///     Prevents data loss of tags or components when storing a scene with entities containing unresolved tag or component types.
///   </item>
/// </list>
/// The reason for unresolved tag or component types can be:<br/>
/// <list type="bullet">
///   <item>
///     Missed to merge C# code containing an <see cref="IEntityTag"/>, an <see cref="IComponent"/> or <see cref="Behavior"/> type definition.
///   </item>
///   <item>
///     Intentionally when creating a scene with external tools without the need to wait for the implementation of new<br/>
///     <see cref="IEntityTag"/>, an <see cref="IComponent"/> or <see cref="Behavior"/> type definitions.
///   </item>
/// </list>
/// </remarks>
[Component("unresolved")]
public struct Unresolved : IComponent
{
    public          UnresolvedComponent[]   components;
    public          string[]                tags;
    
    public override string                  ToString() => GetString(); 
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("unresolved");
        if (components != null) {
            sb.Append(" components: ");
            foreach (var component in components) {
                sb.Append('\'');
                sb.Append(component.key);
                sb.Append("', ");
            }
            sb.Length -= 2;
        }
        if (tags != null) {
            sb.Append(" tags: ");
            foreach (var tag in tags) {
                sb.Append('\'');
                sb.Append(tag);
                sb.Append("', ");
            }
            sb.Length -= 2;
        }
        return sb.ToString();
    }
}

public readonly struct UnresolvedComponent
{
    public readonly     string      key;
    public readonly     JsonValue   value;

    public override string ToString() => $"'{key}': {value.ToString()}";
    
    internal UnresolvedComponent(string key, in JsonValue value) {
        this.key    = key;
        this.value  = new JsonValue(value); // create a copy
    }
}
