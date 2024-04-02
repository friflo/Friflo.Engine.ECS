// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Assign a custom tag name used for JSON serialization for annotated <b>struct</b>s implementing <see cref="ITag"/>.
/// </summary>
/// <remarks>
/// This enables changing a struct name in code without changing the JSON serialization format.
/// </remarks> 
[AttributeUsage(AttributeTargets.Struct)]
public sealed class TagNameAttribute : Attribute {
    public TagNameAttribute (string name) { }
}

/// <summary>
/// Assign a custom key used for JSON serialization for annotated <see cref="IComponent"/> and <see cref="Script"/> types.<br/>
/// If specified key is null The component type is not serialized.
/// </summary>
/// <remarks>
/// The attribute is used for:
/// - annotated structs implementing <see cref="IComponent"/>.<br/>
/// - annotated classes extending <see cref="Script"/>.<br/>
/// <br/>
/// This enables changing a struct / class name in code without changing the JSON serialization format.  
/// </remarks>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class ComponentKeyAttribute : Attribute {
    public ComponentKeyAttribute (string key) { }
}
