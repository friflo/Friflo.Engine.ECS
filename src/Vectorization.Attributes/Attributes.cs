// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Vectorization;

/// <summary>
/// <b>Experimental attribute</b> - Applies vectorization to a method generated with <c>[Query]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class VectorizeAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public sealed class OmitHashAttribute : Attribute { }



