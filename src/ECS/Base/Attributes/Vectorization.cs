using System;

namespace Friflo.Vectorization;

/// <summary>
/// <b>Experimental attribute</b> - Applies vectorization to a method generated with <c>[Query]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class VectorizeAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public sealed class OmitHashAttribute : Attribute { }
