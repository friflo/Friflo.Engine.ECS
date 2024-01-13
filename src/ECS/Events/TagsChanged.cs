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
    public override     string      ToString()  => $"entity: {entityId} - tags change: {tags}";

    public Tags AddedTags { get {
        var result = new Tags();
        result.bitSet.value = ~oldTags.bitSet.value &  tags.bitSet.value;
        return result;
    } }
    
    public Tags RemovedTags { get {
        var result = new Tags();
        result.bitSet.value =  oldTags.bitSet.value & ~tags.bitSet.value;
        return result;
    } }
    
    public Tags ChangedTags { get {
        var result = new Tags();
        result.bitSet.value =  oldTags.bitSet.value ^ tags.bitSet.value;
        return result;
    } }

    internal TagsChangedArgs(EntityStoreBase store, int entityId, in Tags tags, in Tags oldTags)
    {
        this.store      = store as EntityStore;
        this.entityId   = entityId;
        this.tags       = tags;
        this.oldTags    = oldTags;
    }
}