// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="EntitiesChangedHandler"/> added to <see cref="EntityStore.EntitiesChanged"/> get events in
/// case an <see cref="Entity"/> changed.
/// </summary>
public delegate void   EntitiesChangedHandler    (in EntitiesChangedArgs e);

public readonly struct  EntitiesChangedArgs
{
    /// <remarks>
    /// Use <see cref="EntityStore.GetNodeById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetNodeById(args.entityIds[n]).Entity;       </code>
    /// </remarks>
    public readonly     IReadOnlySet<int>   EntityIds  => entityIds;
    
    public readonly     HashSet<int>        entityIds;   //  8
    
    public override     string              ToString() => $"entities changed. Count: {entityIds.Count}";

    public EntitiesChangedArgs(HashSet<int> entityIds)
    {
        this.entityIds = entityIds;
    }
}