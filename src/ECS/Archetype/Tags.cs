// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[CLSCompliant(true)]
public struct Tags : IEnumerable<SchemaType>
{
    internal            BitSet  bitSet;  // 32
    
    public readonly     TagsEnumerator  GetEnumerator()                             => new TagsEnumerator (this);

    // --- IEnumerable
           readonly     IEnumerator     IEnumerable.GetEnumerator()                 => new TagsEnumerator (this);

    // --- IEnumerable<>
    readonly IEnumerator<SchemaType> IEnumerable<SchemaType>.GetEnumerator()  => new TagsEnumerator (this);

    public  readonly override string    ToString() => GetString();
    
    // ----------------------------------------- read Tags ----------------------------------------- 
    public readonly int     Count => bitSet.GetBitCount();
    
    public readonly bool    Has<T> ()
        where T : struct, IEntityTag
    {
        return bitSet.Has(TagType<T>.TagIndex);
    }

    public readonly bool    Has<T1, T2> ()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        return bitSet.Has(TagType<T1>.TagIndex) &&
               bitSet.Has(TagType<T2>.TagIndex);
    }

    public readonly bool    Has<T1, T2, T3> ()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
        where T3 : struct, IEntityTag
    {
        return bitSet.Has(TagType<T1>.TagIndex) &&
               bitSet.Has(TagType<T2>.TagIndex) &&
               bitSet.Has(TagType<T3>.TagIndex);
    }
    
    public readonly bool HasAll (in Tags tags)
    {
        return bitSet.HasAll(tags.bitSet);
    }
    
    public readonly bool HasAny (in Tags tags)
    {
        return bitSet.HasAny(tags.bitSet);
    }
    
    // ----------------------------------------- mutate Tags -----------------------------------------
    internal void SetBit(int tagIndex) {
        bitSet.SetBit(tagIndex);
    }
    
    private void ClearBit(int tagIndex) {
        bitSet.ClearBit(tagIndex);
    }
    
    public void Add<T>()
        where T : struct, IEntityTag
    {
        SetBit(TagType<T>.TagIndex);
    }
    
    public void Add(in Tags tags)
    {
        bitSet.value |= tags.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, IEntityTag
    {
        ClearBit(TagType<T>.TagIndex);
    }
    
    public void Remove(in Tags tags)
    {
        bitSet.value &= ~tags.bitSet.value;
    }
        
    // ----------------------------------------- static methods -----------------------------------------    
    public static Tags Get<T>()
        where T : struct, IEntityTag
    {
        var tags = new Tags();
        tags.SetBit(TagType<T>.TagIndex);
        return tags;
    }
    
    public static Tags Get(SchemaType type)
    {
        var tags = new Tags();
        tags.SetBit(type.tagIndex);
        return tags;
    }
    
    public static Tags Get<T1, T2>()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        var tags = new Tags();
        tags.SetBit(TagType<T1>.TagIndex);
        tags.SetBit(TagType<T2>.TagIndex);
        return tags;
    }
    
    private readonly string GetString()
    {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Tags: [");
        foreach (var index in bitSet) {
            var tagType = EntityStoreBase.Static.ComponentSchema.GetTagAt(index);
            sb.Append('#');
            sb.Append(tagType.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
        }
        sb.Append(']');
        return sb.ToString();
    }
}

public struct TagsEnumerator : IEnumerator<SchemaType>
{
    private BitSetEnumerator    bitSetEnumerator;

    // --- IEnumerator
    public          void        Reset()             => bitSetEnumerator.Reset();

           readonly object      IEnumerator.Current => Current;

    public readonly SchemaType  Current             => EntityStoreBase.Static.ComponentSchema.GetTagAt(bitSetEnumerator.Current);
    
    internal TagsEnumerator(in Tags tags) {
        bitSetEnumerator = tags.bitSet.GetEnumerator();
    }
    
    // --- IEnumerator
    public          bool MoveNext() => bitSetEnumerator.MoveNext();
    public readonly void Dispose() { }
}