// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Assign a custom tag name used for JSON serialization of the annotated structs implementing <see cref="ITag"/>.<br/>
/// <br/>
/// This enables changing a struct name in code without changing the JSON serialization format.  
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class TagNameAttribute : Attribute {
    public TagNameAttribute (string name) { }
}

/// <summary>
/// Assign a custom key used for JSON serialization of<br/>
/// - annotated structs implementing <see cref="IComponent"/>.<br/>
/// - annotated classes extending <see cref="Script"/>.<br/>
/// <br/>
/// This enables changing a struct / class name in code without changing the JSON serialization format.  
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class ComponentKeyAttribute : Attribute {
    public ComponentKeyAttribute (string key) { }
}
