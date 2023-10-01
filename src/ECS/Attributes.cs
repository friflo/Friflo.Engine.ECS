using System;

// ReSharper disable UnusedParameter.Local
namespace Fliox.Engine.ECS;

/// <summary>
/// Annotated structs can be added as component to <see cref="GameEntity"/>'s using the specified key name.<br/>
/// <b><c>struct</c></b> components are the preferred when dealing with a large amount (> 1.000) of <see cref="GameEntity"/>'s. 
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class StructComponentAttribute : Attribute {
    public StructComponentAttribute (string key) { }
}

/// <summary>
/// Annotated classes a can be added as component to <see cref="GameEntity"/>'s using the specified key name.<br/>
/// <b><c>class</c></b> components can be used if <b>OPP</b> programming approach is preferred
/// while dealing with a small amount (&lt; 100) of <see cref="GameEntity"/>'s
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ClassComponentAttribute : Attribute {
    public ClassComponentAttribute (string key) { }
}