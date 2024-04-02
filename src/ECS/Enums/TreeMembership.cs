// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static Friflo.Engine.ECS.StoreOwnership;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Describe the membership of an <see cref="Entity"/> to the <see cref="EntityStore"/> tree graph.
/// </summary>
/// <remarks>Requirement: The entity must be <see cref="attached"/> to an <see cref="EntityStore"/></remarks>
public enum TreeMembership
{
    /// <summary> The entity is not member of the <see cref="EntityStore"/> tree graph. </summary>
    floating    = 0,
    /// <summary> The entity is member of the <see cref="EntityStore"/> tree graph. </summary>
    treeNode    = 1,
}
