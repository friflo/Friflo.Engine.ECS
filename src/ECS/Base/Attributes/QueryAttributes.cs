// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

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