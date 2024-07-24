// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="Unresolved"/> is a container for unresolved entity components.
/// </summary>
/// <remarks>
/// An <see cref="Unresolved"/> component is added to an <see cref="Entity"/> by <see cref="EntityConverter.DataEntityToEntity"/> if:<br/>
/// <list type="bullet">
///   <item>
///     A component in <see cref="DataEntity"/>.<see cref="DataEntity.components"/> cannot be resolved to an <see cref="IComponent"/> or <see cref="Script"/> type. 
///   </item>
///   <item>
///     A tag in <see cref="DataEntity"/>.<see cref="DataEntity.tags"/>  cannot be resolved to an <see cref="ITag"/> type.
///   </item>
/// </list>
/// The <see cref="Unresolved"/> component enables conversion of a <see cref="DataEntity"/> to an <see cref="Entity"/> and vice versa<br/>
/// with components or tags that cannot be resolved to <see cref="ITag"/>, <see cref="IComponent"/> and <see cref="Script"/> types.<br/>
/// <br/>
/// Having support <see cref="Unresolved"/> component or tag types:
/// <list type="bullet">
///   <item>
///     Ensures the ability to use an <see cref="EntityStore"/> containing unresolved types without being blocked by a missing fix.
///   </item>
///   <item>
///     Prevents data loss of tags or components when storing an <see cref="EntityStore"/> with entities containing unresolved tag or component types.
///   </item>
/// </list>
/// The reason for unresolved tag or component types can be:<br/>
/// <list type="bullet">
///   <item>
///     Missed to merge C# code containing an <see cref="ITag"/>, an <see cref="IComponent"/> or <see cref="Script"/> type definition.
///   </item>
///   <item>
///     Intentionally when creating an <see cref="EntityStore"/> with external tools without the need to wait for the implementation of new<br/>
///     <see cref="ITag"/>, an <see cref="IComponent"/> or <see cref="Script"/> type definitions.
///   </item>
/// </list>
/// </remarks>
[ComponentKey("unresolved")]
[ComponentSymbol("Un", "255,0,0")]
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
                sb.Append('\"');
                sb.Append(component.key);
                sb.Append("\", ");
            }
            sb.Length -= 2;
        }
        if (tags != null) {
            sb.Append(" tags: ");
            foreach (var tag in tags) {
                sb.Append('\"');
                sb.Append(tag);
                sb.Append("\", ");
            }
            sb.Length -= 2;
        }
        return sb.ToString();
    }
}

/// <summary>
/// Is used as item type for <see cref="Unresolved"/>.<see cref="Unresolved.components"/> storing unresolved entity components.
/// </summary>
public readonly struct UnresolvedComponent
{
    public readonly     string      key;
    public readonly     JsonValue   value;

    public override string ToString() => $"\"{key}\": {value.ToString()}";
    
    public UnresolvedComponent(string key, in JsonValue value) {
        this.key    = key;
        this.value  = new JsonValue(value); // create a copy
    }
}
