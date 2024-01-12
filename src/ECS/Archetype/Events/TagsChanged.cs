// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct  TagsChangedArgs
{
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetEntityById(args.entityId);       </code>
    /// </remarks>
    public readonly     int     entityId;   //  4
    public readonly     Tags    tags;       // 32

    
    public override     string              ToString() => $"entity: {entityId} - tags change: {tags}";

    internal TagsChangedArgs(int entityId, in Tags tags)
    {
        this.entityId       = entityId;
        this.tags           = tags;
    }
}