// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Assign a custom tag name used for serialization of the annotated structs implementing <see cref="ITag"/>.<br/>
/// <br/>
/// This enables changing the struct name in code without changing the serialization format.  
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class TagAttribute : Attribute {
    public TagAttribute (string name) { }
}

/// <summary>
/// Assign a custom component name used for serialization of the annotated structs implementing <see cref="IComponent"/>.<br/>
/// <br/>
/// This enables changing the struct name in code without changing the serialization format.  
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentAttribute : Attribute {
    public ComponentAttribute (string key) { }
}

/// <summary>
/// Assign a custom script name used for serialization of the annotated class extending <see cref="Script"/>.<br/>
/// <br/>
/// This enables changing the class name in code without changing the serialization format.  
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ScriptAttribute : Attribute {
    public ScriptAttribute (string key) { }
}