// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct ChildEntitiesChangedArgs
{
    public readonly ChildEntitiesChangedAction  action;     //  4
    public readonly EntityStore                 store;      //  8
    public readonly int                         parentId;   //  4
    public readonly int                         childId;    //  4
    public readonly int                         childIndex; //  4
    
    public          Entity                      Parent      => new Entity(parentId, store);
    public          Entity                      Child       => new Entity(childId,  store);

    public override string                      ToString()  => $"entity: {parentId} - event > {action} Child[{childIndex}] = {childId}";

    internal ChildEntitiesChangedArgs(
        ChildEntitiesChangedAction  action,
        EntityStore                 store,
        int                         parentId,
        int                         childId,
        int                         childIndex)
    {
        this.action     = action;
        this.store      = store;
        this.parentId   = parentId;
        this.childId    = childId;
        this.childIndex = childIndex;
    }
}