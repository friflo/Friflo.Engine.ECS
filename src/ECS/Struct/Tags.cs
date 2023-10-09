// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct Tags : IEnumerable<ComponentType>
{
    internal   BitSet  bitSet;
    
    public TagsEnumerator       GetEnumerator()                             => new TagsEnumerator (this);
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator()                 => new TagsEnumerator (this);
    // --- IEnumerable<>
    IEnumerator<ComponentType>  IEnumerable<ComponentType>.GetEnumerator()  => new TagsEnumerator (this);

    public   override string    ToString() => GetString();
    
    private Tags(in BitSet bitSet) {
        this.bitSet = bitSet;
    }
    
    // ----------------------------------------- read Tags ----------------------------------------- 
    public  bool    Has<T> ()
        where T : struct, IEntityTag
    {
        return bitSet.Has(TagType<T>.TagIndex);
    }

    public  bool    Has<T1, T2> ()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        return bitSet.Has(TagType<T1>.TagIndex) &&
               bitSet.Has(TagType<T2>.TagIndex);
    }

    public  bool    Has<T1, T2, T3> ()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
        where T3 : struct, IEntityTag
    {
        return bitSet.Has(TagType<T1>.TagIndex) &&
               bitSet.Has(TagType<T2>.TagIndex) &&
               bitSet.Has(TagType<T3>.TagIndex);
    }
    
    public  bool    HasAll (in Tags tags)
    {
        return bitSet.HasAll(tags.bitSet);
    }
    
    public  bool    HasAny (in Tags tags)
    {
        return bitSet.HasAny(tags.bitSet);
    }
    
    // ----------------------------------------- mutate Tags -----------------------------------------
    
    public void Add<T>()
        where T : struct, IEntityTag
    {
        bitSet.SetBit(TagType<T>.TagIndex);
    }
    
    public void Add(in Tags tags)
    {
        bitSet.value |= tags.bitSet.value;
    }
    
    public void Remove<T>()
        where T : struct, IEntityTag
    {
        bitSet.ClearBit(TagType<T>.TagIndex);
    }
    
    public void Remove(in Tags tags)
    {
        bitSet.value &= ~tags.bitSet.value;
    }
        
    // ----------------------------------------- static methods -----------------------------------------    
    public static Tags Get<T>()
        where T : struct, IEntityTag
    {
        BitSet bitSet = default;
        bitSet.SetBit(TagType<T>.TagIndex);
        return new Tags(bitSet);
    }
    
    public static Tags Get<T1, T2>()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        BitSet bitSet = default;
        bitSet.SetBit(TagType<T1>.TagIndex);
        bitSet.SetBit(TagType<T2>.TagIndex);
        return new Tags(bitSet);
    }
    
    private string GetString()
    {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Tags: [");
        foreach (var index in bitSet) {
            var tagType = EntityStore.Static.ComponentSchema.GetTagAt(index);
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

public struct TagsEnumerator : IEnumerator<ComponentType>
{
    private BitSetEnumerator    bitSetEnumerator;

    // --- IEnumerator
    public  void                Reset()             => bitSetEnumerator.Reset();

            object              IEnumerator.Current => Current;

    public  ComponentType       Current => EntityStore.Static.ComponentSchema.GetTagAt(bitSetEnumerator.Current);
    
    internal TagsEnumerator(in Tags tags) {
        bitSetEnumerator = tags.bitSet.GetEnumerator();
    }
    
    // --- IEnumerator
    public bool MoveNext() => bitSetEnumerator.MoveNext();
    public void Dispose() { }
}