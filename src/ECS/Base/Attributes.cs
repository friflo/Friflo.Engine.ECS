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

/// <summary>
/// Short symbol for a component, tag or script in UI or console as a name (max 3 chars) and a color.<br/>
/// Prefer using single character names for common used symbols. Consider <a href="https://en.wikipedia.org/wiki/List_of_Unicode_characters">Unicode characters</a>.
/// </summary>
/// <remarks>
/// Use 2 or 3 letter strings only for rarely used or important components, tags or scripts.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class ComponentSymbolAttribute : Attribute {
    public ComponentSymbolAttribute (string name, string color = null) { }
}

/// <summary>
/// The attribute is required to register specific type instances of generic component and tags types.
/// See example in remarks.
/// </summary>
/// <remarks>
/// The following example registers a specific generic component instance <c>GenericComponent&lt;int></c>.<br/>
/// The key used for JSON serialization is <c>"comp-int"</c>.
/// <code>
///     [GenericInstanceType("comp-int", typeof(int))] 
///     public struct GenericComponent&lt;T> : IComponent {
///         public T Value;
///     }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class GenericInstanceTypeAttribute : Attribute {
    /// <summary> Register generic component / tag type with one generic parameter. </summary>
    public GenericInstanceTypeAttribute (string componentKey, Type type) { }
    
    /// <summary> Register generic component / tag type with two generic parameters. </summary>
    public GenericInstanceTypeAttribute (string componentKey, Type type1, Type type2) { }
    
    /// <summary> Register generic component / tag type with three generic parameters. </summary>
    public GenericInstanceTypeAttribute (string componentKey, Type type1, Type type2, Type type3) { }
}
