// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct  EntitiesChanged
{
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetEntityById(args.EntityIds[]);       </code>
    /// </remarks>
#if NET5_0_OR_GREATER
    public              IReadOnlySet<int>   EntityIds   => entityIds;
#else
    public              ISet<int>           EntityIds   => entityIds;
#endif
    
    private readonly    HashSet<int>        entityIds;  //  8
    
    public  override    string              ToString()  => $"entities changed. Count: {entityIds.Count}";

    public EntitiesChanged(HashSet<int> entityIds)
    {
        this.entityIds = entityIds;
    }
}