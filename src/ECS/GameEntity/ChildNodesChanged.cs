// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="ChildNodesChangedHandler"/> added to <see cref="GameEntityStore.ChildNodesChanged"/> get events on
/// <list type="bullet">
///   <item><see cref="GameEntity.AddChild"/></item>
///   <item><see cref="GameEntity.InsertChild"/></item>
///   <item><see cref="GameEntity.RemoveChild"/></item>
///   <item><see cref="GameEntity.DeleteEntity"/></item>
/// </list>
/// </summary>
public delegate void   ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs e);

public readonly struct ChildNodesChangedArgs
{
    public readonly ChildNodesChangedAction action;
    public readonly int                     parentId;
    public readonly int                     childId;
    public readonly int                     childIndex;

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