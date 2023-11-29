// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="ComponentAddedHandler"/> added to <see cref="EntityStore.ComponentAdded"/>
/// get events on <see cref="Entity.AddComponent{T}()"/>
/// </summary>
public delegate void   ComponentAddedHandler    (in ComponentChangedArgs e);

/// <summary>
/// A <see cref="ComponentRemovedHandler"/> added to <see cref="EntityStore.ComponentRemoved"/>
/// get events on <see cref="Entity.RemoveComponent{T}()"/>
/// </summary>
public delegate void   ComponentRemovedHandler  (in ComponentChangedArgs e);


public enum ChangedEventType
{
    Added,
    Removed,
}

public readonly struct  ComponentChangedArgs
{
    public readonly     int                 entityId;
    public readonly     ChangedEventType    type; 
    public readonly     ComponentType       componentType;
    
    public override     string              ToString() => $"entity: {entityId} - {type} {componentType}";

    internal ComponentChangedArgs(int entityId, ChangedEventType type, int structIndex)
    {
        this.entityId       = entityId;
        this.type           = type;
        this.componentType  = EntityStoreBase.Static.EntitySchema.components[structIndex];
    }
}