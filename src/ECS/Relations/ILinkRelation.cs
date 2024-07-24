// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A link relation is a component type used to create multiple links from one entity to other entities.
/// </summary>
/// <remarks>
/// A link relation enables:
/// <list type="bullet">
///   <item>
///     Add multiple link relations to an entity using <see cref="Entity.AddComponent{T}()"/>.
///   </item>
///   <item>
///     Return all links of an entity to other entities using <see cref="RelationExtensions.GetRelations{TComponent}"/>.
///   </item>
///   <item>
///     Remove a specific link to another entity with <see cref="RelationExtensions.RemoveRelation{T}"/>.
///   </item>
/// </list>
/// </remarks>
public interface ILinkRelation : IRelationComponent<Entity> {
}