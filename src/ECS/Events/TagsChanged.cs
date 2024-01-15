// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Text;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct  TagsChanged
{
    public  readonly    EntityStore Store;      //  8
    public  readonly    int         EntityId;   //  4
    public  readonly    Tags        Tags;       // 32
    public  readonly    Tags        OldTags;    // 32
    
    public              Entity      Entity      => new Entity(Store, EntityId);
    
    public              Tags        AddedTags   => new(~OldTags.bitSet.value &  Tags.bitSet.value);
    public              Tags        RemovedTags => new( OldTags.bitSet.value & ~Tags.bitSet.value);
    public              Tags        ChangedTags => new( OldTags.bitSet.value ^  Tags.bitSet.value);
    
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
        if (added.bitSet.value != default) {
            sb.Append(" Add ");
            sb.Append(added.ToString());
        }
        var removed = RemovedTags;
        if (removed.bitSet.value != default) {
            sb.Append(" Remove ");
            sb.Append(removed.ToString());
        }
        return sb.ToString();
    }
}