// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public interface IRelation { }

/// <summary>
/// A relation enables adding multiple components of the same type to an entity.<br/>
/// The components added to a single entity build a set of components using the relation <typeparamref name="TKey"/> as unique identifier.  
/// </summary>
/// <typeparam name="TKey">The key defining a unique relation.</typeparam>
/// <remarks>
/// A relation enables:
/// <list type="bullet">
///   <item>
///     Add multiple relations to an entity using <see cref="RelationExtensions.AddRelation{TRelation}"/>.
///   </item>
///   <item>
///     Return all relations of an entity using <see cref="RelationExtensions.GetRelations{TRelation}"/>.
///   </item>
///   <item>
///     Return a specific relation by key using  <see cref="RelationExtensions.GetRelation{TRelation,TKey}"/><br/>
///     or <see cref="RelationExtensions.TryGetRelation{TRelation,TKey}"/>.
///   </item>
///   <item>
///     Remove a specific relation by key using <see cref="RelationExtensions.RemoveRelation{T,TKey}"/>.
///   </item>
/// </list>
/// </remarks>
public interface IRelation<out TKey> : IRelation
{
    /// <summary>
    /// Returns the key of a unique relation.
    /// </summary>
    TKey GetRelationKey();
}