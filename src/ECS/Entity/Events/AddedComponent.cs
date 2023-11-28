// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="AddedComponentHandler"/> added to <see cref="EntityStore.AddedComponent"/> get events on <see cref="Entity.AddComponent{T}()"/>
/// </summary>
public delegate void   AddedComponentHandler(object sender, in AddedComponentArgs e);

public readonly struct  AddedComponentArgs
{
    public readonly     int             entityId;
    public readonly     ComponentType   componentType;
    
    public override     string          ToString() => $"entity: {entityId} - add: {componentType}";

    internal AddedComponentArgs(int entityId, ComponentType  componentType)
    {
        this.entityId       = entityId;
        this.componentType  = componentType;
    }
}