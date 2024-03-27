// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="EntityStore.OnEntityDeleted"/>.
/// </summary>
/// <remarks>
/// These events are fired on <see cref="ECS.Entity.DeleteEntity"/>.
/// </remarks>
public readonly struct  EntityDeleted
{
#region fields
    /// <summary>The deleted <see cref="Entity"/>.</summary>
    public  readonly    Entity      Entity;         // 16

    #endregion
    
#region properties
    /// <summary>The <see cref="EntityStore"/> of the deleted entity.</summary>
    public              EntityStore Store       => Entity.store;
    
    public override     string      ToString()  => $"entity: {Entity.Id} - event > EntityDeleted";
    #endregion

#region methods
    internal EntityDeleted(Entity entity)
    {
        Entity = entity;
    }
    #endregion
}