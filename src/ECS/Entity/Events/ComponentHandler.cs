// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="AddedComponentHandler"/> added to <see cref="EntityStore.AddedComponent"/>
/// get events on <see cref="Entity.AddComponent{T}()"/>
/// </summary>
public delegate void   AddedComponentHandler    (in ComponentEventArgs e);

/// <summary>
/// A <see cref="RemovedComponentHandler"/> added to <see cref="EntityStore.RemovedComponent"/>
/// get events on <see cref="Entity.RemoveComponent{T}()"/>
/// </summary>
public delegate void   RemovedComponentHandler  (in ComponentEventArgs e);


public enum ComponentEventType
{
    Added,
    Removed,
}

public readonly struct  ComponentEventArgs
{
    public readonly     int                 entityId;
    public readonly     ComponentEventType  type; 
    public readonly     ComponentType       componentType;
    
    public override     string              ToString() => $"entity: {entityId} - {type} {componentType}";

    internal ComponentEventArgs(int entityId, ComponentEventType type, ComponentType  componentType)
    {
        this.entityId       = entityId;
        this.type           = type;
        this.componentType  = componentType;
    }
}