// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;


public delegate void   NotifyChildNodesChangedEventHandler(object sender, in NotifyChildNodesChangedEventArgs e);

public readonly struct NotifyChildNodesChangedEventArgs
{
    public readonly NotifyChildNodesChangedAction  action;
    public readonly int                            entityId;
    public readonly int                            index;
    
    internal NotifyChildNodesChangedEventArgs(
        NotifyChildNodesChangedAction  action,
        int                            entityId,
        int                            index)
    {
        this.action     = action;
        this.entityId   = entityId;
        this.index      = index;
    }
}