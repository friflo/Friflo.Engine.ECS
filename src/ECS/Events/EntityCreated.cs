// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="EntityStore.OnEntityCreated"/>.
/// </summary>
/// <remarks>
/// These events are fired on <see cref="EntityStore.CreateEntity()"/>.
/// </remarks>
public readonly struct  EntityCreated
{
#region fields
    /// <summary>The created <see cref="ECS.Entity"/>.</summary>
    public  readonly    Entity      Entity;         // 16

    #endregion
    
#region properties
    /// <summary>The <see cref="EntityStore"/> containing the created entity.</summary>
    public              EntityStore Store       => Entity.store;
    
    public override     string      ToString()  => $"entity: {Entity.Id} - event > EntityCreated";
    #endregion

#region methods
    internal EntityCreated(Entity entity)
    {
        Entity = entity;
    }
    #endregion
}