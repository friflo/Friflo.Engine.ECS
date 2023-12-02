// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Declares the <see cref="SchemaType.kind"/> of a <see cref="SchemaType"/> 
/// </summary>
public enum SchemaTypeKind
{
    /// <summary>Declare a <see cref="ComponentType"/> is an <b><see cref="IComponent"/></b></summary>
    Component   = 0,
    /// <summary>Declare a <see cref="ScriptType"/> is a <b><see cref="Script"/></b></summary>
    Script      = 1,
    /// <summary>Declare a <see cref="TagType"/> is an <b><see cref="IEntityTag"/></b></summary>
    /// <remarks>A <b>Tag</b> is defined by struct definition without fields / properties extending <see cref="IEntityTag"/></remarks>
    Tag         = 2
}
