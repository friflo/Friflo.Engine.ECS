// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Text;
using Friflo.Engine.ECS.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="Entity.OnTagsChanged"/> or <see cref="EntityStoreBase.OnTagsChanged"/>.
/// </summary>
/// <remarks>
/// These events are fired on:
/// <list type="bullet">
///     <item><see cref="Entity.AddTag{TTag}"/></item>
///     <item><see cref="Entity.AddTags"/></item>
///     <item><see cref="Entity.RemoveTag{TTag}"/></item>
///     <item><see cref="Entity.RemoveTags"/></item>
/// </list>
/// </remarks>
public readonly struct  TagsChanged
{
    /// <summary>The <see cref="EntityStore"/> containing the <see cref="Entity"/> that emitted the event.</summary>
    public  readonly    EntityStore Store;      //  8
    
    /// <summary>The <c>Id</c> of the <see cref="Entity"/> that emitted the event.</summary>
    public  readonly    int         EntityId;   //  4
    
    /// <summary>The new state of the <see cref="Entity"/> <see cref="ECS.Entity.Tags"/>.</summary>
    public  readonly    Tags        Tags;       // 32
    
    /// <summary>The old state of the <see cref="Entity"/> <see cref="ECS.Entity.Tags"/> before the change.</summary>
    public  readonly    Tags        OldTags;    // 32
    
    // --- properties
    /// <summary>The <see cref="Entity"/> that emitted the event - aka the publisher.</summary>
    public              Entity      Entity      => new Entity(Store, EntityId);
    
    /// <summary>The <see cref="ECS.Tags"/> added to the <see cref="Entity"/>.</summary>
    public              Tags        AddedTags   => new(BitSet.Added  (OldTags.bitSet, Tags.bitSet));
    
    /// <summary>The <see cref="ECS.Tags"/> removed from the <see cref="Entity"/>.</summary>
    public              Tags        RemovedTags => new(BitSet.Removed(OldTags.bitSet, Tags.bitSet));
    
    /// <summary>The changed (removed / added) entity <see cref="ECS.Tags"/>.</summary>
    public              Tags        ChangedTags => new(BitSet.Changed(OldTags.bitSet, Tags.bitSet));
    
    public override     string      ToString()  => GetString();

    internal TagsChanged(EntityStoreBase store, int entityId, in Tags tags, in Tags oldTags)
    {
        this.Store      = store as EntityStore;
        this.EntityId   = entityId;
        this.Tags       = tags;
        this.OldTags    = oldTags;
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        sb.Append("entity: ");
        sb.Append(EntityId);
        sb.Append(" - event >");
        var added = AddedTags;
        if (!added.bitSet.IsDefault()) {
            sb.Append(" Add ");
            sb.Append(added.ToString());
        }
        var removed = RemovedTags;
        if (!removed.bitSet.IsDefault()) {
            sb.Append(" Remove ");
            sb.Append(removed.ToString());
        }
        return sb.ToString();
    }
}