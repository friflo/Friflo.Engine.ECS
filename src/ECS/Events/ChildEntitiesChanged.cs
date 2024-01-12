// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct ChildEntitiesChangedArgs
{
    public readonly ChildEntitiesChangedAction  action;     //  4
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetEntityById(args.entityId);       </code>
    /// </remarks>
    public readonly int                         parentId;   //  4
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.
    /// <code>      var entity = store.GetEntityById(args.entityId);       </code>
    /// </remarks>
    public readonly int                         childId;    //  4
    public readonly int                         childIndex; //  4

    public override string                      ToString() => $"entity: {parentId} - {action} ChildIds[{childIndex}] = {childId}";

    internal ChildEntitiesChangedArgs(
        ChildEntitiesChangedAction  action,
        int                         parentId,
        int                         childId,
        int                         childIndex)
    {
        this.action     = action;
        this.parentId   = parentId;
        this.childId    = childId;
        this.childIndex = childIndex;
    }
}