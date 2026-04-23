// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Uses a tiled memory layout for internal storage of components types with a single <c>value</c> field of Type
/// <see cref="Vector2"/>, <see cref="Vector3"/> or <see cref="Vector4"/>.<br/>
/// This memory layout is crucial for high performance vectorized execution using the
/// <see href="https://github.com/friflo/Friflo.Vectorization">Friflo.Vectorization</see> source generator.
/// </summary>
/// <remarks>
/// Tiled memory layout.
/// <code>
///     // e.g. for Vector2
///     [AoSoA] public struct Position2 : IComponent { public Vector2 value; }
///     // layouts
///     Vector2:   xxxxxxxx yyyyyyyy   xxxxxxxx yyyyyyyy   ...
///     Vector3:   xxxxxxxx yyyyyyyy zzzzzzzz   xxxxxxxx yyyyyyyy zzzzzzzz   ...
///     Vector3:   xxxxxxxx yyyyyyyy zzzzzzzz wwwwwwww   xxxxxxxx yyyyyyyy zzzzzzzz wwwwwwww   ...
///     x, y, z, w are floats stored in a single  float[] array
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class AoSoAAttribute : Attribute { }


/// <summary>
/// Uses a strides memory layout for internal storage of components types with a single <c>value</c> field of Type
/// <see cref="Vector2"/>, <see cref="Vector3"/> or <see cref="Vector4"/>.<br/>
/// This memory layout is currently <b>NOT</b> supported by
/// <see href="https://github.com/friflo/Friflo.Vectorization">Friflo.Vectorization</see> source generator.
/// </summary>
/// <remarks>
/// Strided memory layout.
/// <code>
///     // e.g. for Vector2
///     [SoA] public struct Position2 : IComponent { public Vector2 value; }
///     // layouts
///     Vector2:   xxxxxxxx xxxxxxxx ...   yyyyyyyy yyyyyyyy ...
///     Vector3:   xxxxxxxx xxxxxxxx ...   yyyyyyyy yyyyyyyy ...   zzzzzzzz ...
///     Vector3:   xxxxxxxx yyyyyyyy zzzzzzzz wwwwwwww   xxxxxxxx yyyyyyyy zzzzzzzz wwwwwwww   ...
///     x, y, z, w are floats stored in a single float[] array
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class SoAAttribute : Attribute { }


/// <summary>
/// Creates an additional query method for the annotated method. New method has suffix <c>...Query()</c>.<br/>
/// The generated query method filters all components declared as parameters + additional filters added with attributes.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization#query-generator">Query Generator.</a>
/// </summary>
/// <remarks>
/// The parameter values of the annotated <c>[Query]</c> method depend on their <c>Type</c>:
/// <list type="bullet">
///   <item> Component types      - the component values of a matching entity. </item>
///   <item> <c>Entity entity</c> - the current iterated entity. </item>
///   <item> Other types          - are set by the values passed to generated <c>...Query()</c> method call. </item>
/// </list>
/// Component filter attributes
/// <code>
///     [AllComponents&lt;>]
///     [AnyComponents&lt;>]
///     [WithoutAllComponents&lt;>]
///     [WithoutAnyComponents&lt;>]
/// </code>
/// Tag filter attributes
/// <code>
///     [AllTags&lt;>]
///     [AnyTags&lt;>]
///     [WithoutAllTags&lt;>]
///     [WithoutAnyTags&lt;>]
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryAttribute : Attribute { }