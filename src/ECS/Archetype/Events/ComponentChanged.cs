// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A <see cref="ComponentChangedHandler"/> added to <see cref="EntityStoreBase.OnComponentAdded"/> get events on <see cref="Entity.AddComponent{T}()"/><br/>
/// <br/>
/// A <see cref="ComponentChangedHandler"/> added to <see cref="EntityStoreBase.OnComponentRemoved"/> get events on <see cref="Entity.RemoveComponent{T}()"/>
/// </summary>
public delegate void   ComponentChangedHandler    (in ComponentChangedArgs e);

public enum ChangedEventAction
{
    Add     = 0,
    Remove  = 1,
}

public readonly struct  ComponentChangedArgs
{
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetEntityById(args.entityId);       </code>
    /// </remarks>
    public readonly     int                 entityId;       //  4
    public readonly     ChangedEventAction  action;         //  4
    public readonly     ComponentType       componentType;  //  8
    
    public override     string              ToString() => $"entity: {entityId} - {action} {componentType}";

    internal ComponentChangedArgs(int entityId, ChangedEventAction action, int structIndex)
    {
        this.entityId       = entityId;
        this.action         = action;
        this.componentType  = EntityStoreBase.Static.EntitySchema.components[structIndex];
    }
}