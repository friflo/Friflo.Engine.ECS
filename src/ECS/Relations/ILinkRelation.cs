// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A link relation is a component type used to create multiple links from one entity to other entities.
/// </summary>
/// <remarks>
/// A link relationenables:
/// <list type="bullet">
///   <item>
///     Return all link relations of an entity using <see cref="RelationExtensions.GetRelations{TComponent}"/>.
///   </item>
///   <item>
///     Remove a specific link relation to another entity with <see cref="RelationExtensions.RemoveLinkRelation{T}"/>.
///   </item>
/// </list>
/// </remarks>
internal interface ILinkRelation : IRelationComponent<Entity> {
}