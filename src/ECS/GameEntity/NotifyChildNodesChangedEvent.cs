// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;


public delegate void   NotifyChildNodesChangedEventHandler(object sender, in NotifyChildNodesChangedEventArgs e);

public readonly struct NotifyChildNodesChangedEventArgs
{
    public readonly NotifyChildNodesChangedAction  action;
    public readonly int                            parentId;
    public readonly int                            childId;
    public readonly int                            childIndex;
    
    internal NotifyChildNodesChangedEventArgs(
        NotifyChildNodesChangedAction  action,
        int                            parentId,
        int                            childId,
        int                            childIndex)
    {
        this.action     = action;
        this.parentId   = parentId;
        this.childId    = childId;
        this.childIndex = childIndex;
    }
}