// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="TagsChangedHandler"/> added to <see cref="EntityStore.TagsChanged"/>
/// </summary>
public delegate void   TagsChangedHandler    (in TagsChangedArgs e);


public readonly struct  TagsChangedArgs
{
    public readonly     int     entityId;   //  4
    public readonly     Tags    tags;       // 32

    
    public override     string              ToString() => $"entity: {entityId} - tags change: {tags}";

    internal TagsChangedArgs(int entityId, in Tags tags)
    {
        this.entityId       = entityId;
        this.tags           = tags;
    }
}