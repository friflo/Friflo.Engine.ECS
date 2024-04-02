// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Declares the <see cref="SchemaType.Kind"/> of a <see cref="SchemaType"/> 
/// </summary>
public enum SchemaTypeKind : byte
{
    /// <summary> Declare a <see cref="ComponentType"/> is an <see cref="IComponent"/>. </summary>
    Component   = 0,
    /// <summary> Declare a <see cref="ScriptType"/> is a <see cref="Script"/>. </summary>
    Script      = 1,
    /// <summary> Declare a <see cref="TagType"/> is an <see cref="ITag"/>. </summary>
    /// <remarks> A <b>Tag</b> is defined by struct definition without fields / properties extending <see cref="ITag"/>. </remarks>
    Tag         = 2
}
