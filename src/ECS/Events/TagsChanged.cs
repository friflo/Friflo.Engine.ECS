// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

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
    
    public override     string      ToString()  => $"entity: {entityId} - tags change: {tags}";

    internal TagsChangedArgs(EntityStoreBase store, int entityId, in Tags tags, in Tags oldTags)
    {
        this.store      = store as EntityStore;
        this.entityId   = entityId;
        this.tags       = tags;
        this.oldTags    = oldTags;
    }
}