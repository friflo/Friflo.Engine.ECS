// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is used to define a component type having a single indexed field / property.<br/>
/// Indexed component values can be queried with <c>HasValue()</c> or <c>ValueInRange()</c> in a <c>Query()</c>.
/// </summary>
/// <remarks>
/// This component type enables:
/// <list type="bullet">
///   <item>
///     Add an index component to an entity using <see cref="Entity.AddComponent{T}()"/>.
///   </item>
///   <item>
///     Return all entities with a component field of a specific value. <br/>
///     See <see cref="IndexExtensions.GetEntitiesWithComponentValue{TIndexedComponent,TValue}"/>.
///   </item>
///   <item>
///     Return a collection of all unique component values.<br/>
///     See <see cref="IndexExtensions.GetAllIndexedComponentValues{TComponent,TValue}"/>.
///   </item>
///   <item>
///     Filter entities in a query having a specific component value.<br/>
///     See <see cref="ArchetypeQuery.HasValue{TComponent,TValue}"/>.
///   </item>
///   <item>
///     Filter entities in a query with a component value in a specific range.<br/>
///     See <see cref="ArchetypeQuery.ValueInRange{TComponent,TValue}"/>.
///   </item>
/// </list>
/// </remarks>
public interface IIndexedComponent<out TValue> : IComponent
{
    /// <summary>
    /// Returns the value of the indexed component field. 
    /// </summary>
    TValue GetIndexedValue();
}
