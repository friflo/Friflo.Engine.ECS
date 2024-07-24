// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>Describe the ownership state of an <see cref="Entity"/></summary>
public enum StoreOwnership
{
    /// <summary> The entity is not owned by an <see cref="EntityStore"/>. </summary>
    /// <remarks>
    /// When calling <see cref="Entity.DeleteEntity"/> its state changes to <see cref="detached"/>.<br/>
    /// </remarks>
    detached    = 0,
    /// <summary> The entity is owned by an <see cref="EntityStore"/>. </summary>
    /// <remarks>
    /// Entities created with <see cref="EntityStore.CreateEntity()"/> are automatically <see cref="attached"/> to its <see cref="EntityStore"/><br/>
    /// </remarks>
    attached    = 1
}


