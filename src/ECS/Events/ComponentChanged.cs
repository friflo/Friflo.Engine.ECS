// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public enum ChangedEventAction
{
    Add     = 0,
    Remove  = 1,
}

public readonly struct  ComponentChanged
{
    public readonly     EntityStore         store;          //  8
    public readonly     int                 entityId;       //  4
    public readonly     ChangedEventAction  action;         //  4
    public readonly     ComponentType       componentType;  //  8
    
    public              Entity              Entity      => new Entity(entityId, store);
    
    public override     string              ToString()  => $"entity: {entityId} - event > {action} {componentType}";

    internal ComponentChanged(EntityStoreBase store, int entityId, ChangedEventAction action, int structIndex)
    {
        this.store          = store as EntityStore; 
        this.entityId       = entityId;
        this.action         = action;
        this.componentType  = EntityStoreBase.Static.EntitySchema.components[structIndex];
    }
}