// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>Describe the ownership state of a <see cref="GameEntity"/></summary>
public enum StoreOwnership
{
    /// <summary>The entity is not owned by an <see cref="GameEntityStore"/></summary>
    /// <remarks>
    /// When calling <see cref="GameEntity.DeleteEntity"/> its state changes to <see cref="detached"/>.<br/>
    /// </remarks>
    detached    = 0,
    /// <summary>The entity is owned by an <see cref="GameEntityStore"/></summary>
    /// <remarks>
    /// Entities created with <see cref="GameEntityStore.CreateEntity()"/> are automatically <see cref="attached"/> to its <see cref="GameEntityStore"/><br/>
    /// </remarks>
    attached    = 1
}


