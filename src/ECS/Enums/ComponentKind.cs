// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// Declares the <see cref="ComponentType.kind"/> of a <see cref="ComponentType"/> 
/// </summary>
public enum ComponentKind
{
    /// <summary>Declare a <see cref="ComponentType"/> is a <b>struct</b> component</summary>
    Struct      = 0,
    /// <summary>Declare a <see cref="ComponentType"/> is a <b>class</b> component</summary>
    Class       = 1
}
