// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Text;
using Friflo.Engine.ECS.Utils;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[CLSCompliant(true)]
public struct Tags : IEnumerable<TagType>
{
    internal            BitSet  bitSet;  // 32
    
    public readonly     TagsEnumerator  GetEnumerator()                         => new TagsEnumerator (this);

    // --- IEnumerable
           readonly     IEnumerator     IEnumerable.GetEnumerator()             => new TagsEnumerator (this);

    // --- IEnumerable<>
    readonly IEnumerator<TagType>       IEnumerable<TagType>.GetEnumerator()    => new TagsEnumerator (this);

    public  readonly override string    ToString() => GetString();
    
    // ----------------------------------- specialized constructor  -----------------------------------
    public Tags(TagType type)
    {
        bitSet.SetBit(type.TagIndex);
    }
    
    public Tags(in Vector256<long> value)
    {
        bitSet.value = value;
    }
    
    // ----------------------------------------- read Tags ----------------------------------------- 
    public readonly int     Count => bitSet.GetBitCount();
    
    /// <summary>Return true if the <see cref="Tags"/> contain the passed tag <typeparamref name="T"/>.</summary>
    public readonly bool    Has<T> ()
        where T : struct, ITag
    {
        return bitSet.Has(TagType<T>.TagIndex);
    }

    /// <summary>Return true if the <see cref="Tags"/> contain the passed tags.</summary>
    public readonly bool    Has<T1, T2> ()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        return bitSet.Has(TagType<T1>.TagIndex) &&
               bitSet.Has(TagType<T2>.TagIndex);
    }

    /// <summary>Return true if the <see cref="Tags"/> contain the passed tags.</summary>
    public readonly bool    Has<T1, T2, T3> ()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
    {
        return bitSet.Has(TagType<T1>.TagIndex) &&
               bitSet.Has(TagType<T2>.TagIndex) &&
               bitSet.Has(TagType<T3>.TagIndex);
    }
    
    /// <summary>Return true if the <see cref="Tags"/> contain all passed <paramref name="tags"/>.</summary>
    public readonly bool HasAll (in Tags tags)
    {
        return bitSet.HasAll(tags.bitSet);
    }
    
    public readonly bool HasAny (in Tags tags)
    {
        return bitSet.HasAny(tags.bitSet);
    }
    
    // ----------------------------------------- mutate Tags -----------------------------------------
    public void Add<T>()
        where T : struct, ITag
    {
        bitSet.SetBit(TagType<T>.TagIndex);
    }
    
    public void Add(in Tags tags)
    {
        bitSet.value |= tags.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, ITag
    {
        bitSet.ClearBit(TagType<T>.TagIndex);
    }
    
    public void Remove(in Tags tags)
    {
        bitSet.value &= ~tags.bitSet.value;
    }
        
    // ----------------------------------------- static methods -----------------------------------------    
    public static Tags Get<T>()
        where T : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagType<T>.TagIndex);
        return tags;
    }
    
    public static Tags Get<T1, T2>()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagType<T1>.TagIndex);
        tags.bitSet.SetBit(TagType<T2>.TagIndex);
        return tags;
    }
    
    private readonly string GetString()
    {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Tags: [");
        foreach (var index in bitSet) {
            var tagType = EntityStoreBase.Static.EntitySchema.GetTagAt(index);
            sb.Append('#');
            sb.Append(tagType.Name);
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

public struct TagsEnumerator : IEnumerator<TagType>
{
    private BitSetEnumerator    bitSetEnumerator;   // 48

    // --- IEnumerator
    public          void        Reset()             => bitSetEnumerator.Reset();

           readonly object      IEnumerator.Current => Current;

    public readonly TagType     Current             => EntityStoreBase.Static.EntitySchema.GetTagAt(bitSetEnumerator.Current);
    
    internal TagsEnumerator(in Tags tags) {
        bitSetEnumerator = new BitSetEnumerator(tags.bitSet);
    }
    
    // --- IEnumerator
    public          bool MoveNext() => bitSetEnumerator.MoveNext();
    public readonly void Dispose() { }
}