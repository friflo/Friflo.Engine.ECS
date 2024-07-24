// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="EntityStore.OnEntityDelete"/>.
/// </summary>
/// <remarks>
/// These events are fired on <see cref="ECS.Entity.DeleteEntity"/>.
/// </remarks>
public readonly struct  EntityDelete
{
#region fields
    /// <summary>The entity that will be deleted.</summary>
    public  readonly    Entity      Entity;         // 16

    #endregion
    
#region properties
    /// <summary>The <see cref="EntityStore"/> of the entity that will be deleted.</summary>
    public              EntityStore Store       => Entity.store;
    
    public override     string      ToString()  => $"entity: {Entity.Id} - event > EntityDelete";
    #endregion

#region methods
    internal EntityDelete(Entity entity)
    {
        Entity = entity;
    }
    #endregion
}