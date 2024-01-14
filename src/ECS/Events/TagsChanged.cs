// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct  TagsChangedArgs
{
    public  readonly    EntityStore store;      //  8
    public  readonly    int         entityId;   //  4
    public  readonly    Tags        tags;       // 32
    public  readonly    Tags        oldTags;    // 32
    
    public              Entity      Entity      => new Entity(entityId, store);
    
    public              Tags        AddedTags   => new(~oldTags.bitSet.value &  tags.bitSet.value);
    public              Tags        RemovedTags => new( oldTags.bitSet.value & ~tags.bitSet.value);
    public              Tags        ChangedTags => new( oldTags.bitSet.value ^  tags.bitSet.value);
    
    public override     string      ToString()  => GetString();

    internal TagsChangedArgs(EntityStoreBase store, int entityId, in Tags tags, in Tags oldTags)
    {
        this.store      = store as EntityStore;
        this.entityId   = entityId;
        this.tags       = tags;
        this.oldTags    = oldTags;
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        sb.Append("entity: ");
        sb.Append(entityId);
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