// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

namespace Friflo.Engine.ECS.Index;

/// <summary>
/// Assigns a custom <see cref="ComponentIndex"/> to an attributed component type.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentIndexAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local
    public ComponentIndexAttribute(Type type) { }
}

