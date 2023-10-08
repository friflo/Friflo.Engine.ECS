// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct Tags
{
    private  readonly   BitSet256   bitSet;
    
    public   override   string      ToString() => GetString();

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
    
    public void ForEach(Action<ComponentType> lambda)
    {
        bitSet.ForEach((position) => {
            var tagType = EntityStore.Static.ComponentSchema.GetTagAt(position);
            lambda(tagType);
        });
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        bitSet.ForEach((position) => {
            var tagType = EntityStore.Static.ComponentSchema.GetTagAt(position);
            sb.Append('#');
            sb.Append(tagType.type.Name);
            sb.Append(", ");
        });
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}

