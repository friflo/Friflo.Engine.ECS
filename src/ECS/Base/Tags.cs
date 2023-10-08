// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct Tags
{
    internal readonly   BitSet256       bitSet;
    
    public              TagsEnumerator  GetEnumerator() => new (this);
    
    public   override   string          ToString() => GetString();
    
    private Tags(in BitSet256 bitSet) {
        this.bitSet = bitSet;
    }
        
    public static Tags Get<T>()
        where T : struct, IEntityTag
    {
        BitSet256 bitSet = default;
        bitSet.SetBit(TagTypeInfo<T>.TagIndex);
        return new Tags(bitSet);
    }
    
    public static Tags Get<T1, T2>()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        BitSet256 bitSet = default;
        bitSet.SetBit(TagTypeInfo<T1>.TagIndex);
        bitSet.SetBit(TagTypeInfo<T2>.TagIndex);
        return new Tags(bitSet);
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var index in bitSet) {
            var tagType = EntityStore.Static.ComponentSchema.GetTagAt(index);
            sb.Append('#');
            sb.Append(tagType.type.Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}

public struct TagsEnumerator
{
    private BitSet256Enumerator bitSetEnumerator;
    
    public  ComponentType   Current => EntityStore.Static.ComponentSchema.GetTagAt(bitSetEnumerator.Current);
    
    internal TagsEnumerator(in Tags tags) {
        bitSetEnumerator = tags.bitSet.GetEnumerator();
    }
    
    // --- IEnumerator
    public bool MoveNext() => bitSetEnumerator.MoveNext();
}