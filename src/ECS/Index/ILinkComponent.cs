// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A link component is a component type used to create a single link from one entity to another entity.<br/>
/// Specific component links can be queried with <c>HasValue()</c> in a <c>Query()</c>.
/// </summary>
/// <remarks>
/// This component type enables:
/// <list type="bullet">
///   <item>
///     Add a component link to an entity using <see cref="Entity.AddComponent{T}()"/>.
///   </item>
///   <item>
///     Return all entities having a <see cref="ILinkComponent"/> to a specific entity.<br/>
///     See <see cref="IndexExtensions.GetIncomingLinks{TComponent}"/>
///   </item>
///   <item>
///     Return all entities linked by a specific <see cref="ILinkComponent"/> type.<br/>
///     See <see cref="IndexExtensions.GetAllLinkedEntities{TComponent}"/>
///   </item>
///   <item>
///     Filter entities in a query having a <see cref="ILinkComponent"/> to a specific entity.<br/>
///     See <see cref="ArchetypeQuery.HasValue{TComponent,TValue}"/>.
///   </item>
/// </list>
/// </remarks>
public interface ILinkComponent : IIndexedComponent<Entity> { }