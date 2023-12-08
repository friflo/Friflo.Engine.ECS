// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="ChildNodesChangedHandler"/> added to <see cref="EntityStore.ChildNodesChanged"/> get events on
/// <list type="bullet">
///   <item><see cref="Entity.AddChild"/></item>
///   <item><see cref="Entity.InsertChild"/></item>
///   <item><see cref="Entity.RemoveChild"/></item>
///   <item><see cref="Entity.DeleteEntity"/></item>
/// </list>
/// </summary>
public delegate void   ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs e);

public readonly struct ChildNodesChangedArgs
{
    public readonly ChildNodesChangedAction action;     //  4
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetNodeById(args.entityId).Entity;       </code>
    /// </remarks>
    public readonly int                     parentId;   //  4
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.
    /// <code>      var entity = store.GetNodeById(args.entityId).Entity;       </code>
    /// </remarks>
    public readonly int                     childId;    //  4
    public readonly int                     childIndex; //  4

    public override string                  ToString() => $"entity: {parentId} - {action} ChildIds[{childIndex}] = {childId}";

    internal ChildNodesChangedArgs(
        ChildNodesChangedAction action,
        int                     parentId,
        int                     childId,
        int                     childIndex)
    {
        this.action     = action;
        this.parentId   = parentId;
        this.childId    = childId;
        this.childIndex = childIndex;
    }
}