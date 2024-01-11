// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A <see cref="TagsChangedHandler"/> added to <see cref="EntityStoreBase.TagsChanged"/> get events on
/// <list type="bullet">
///   <item><see cref="Entity.AddTag{T}"/></item>
///   <item><see cref="Entity.AddTags"/></item>
///   <item><see cref="Entity.RemoveTag{T}"/></item>
///   <item><see cref="Entity.RemoveTags"/></item>
/// </list>
/// </summary>
public delegate void   TagsChangedHandler    (in TagsChangedArgs e);


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