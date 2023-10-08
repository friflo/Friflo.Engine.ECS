// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct Tags
{
    internal readonly   Vector256Long   bitSet;

    public   override   string          ToString() => bitSet.AppendString(new StringBuilder()).ToString();

    private Tags(in Vector256Long bitSet) {
        this.bitSet = bitSet;
    }
        
    public static Tags Get<T>()
        where T : struct, IEntityTag
    {
        Vector256Long bitSet = default;
        bitSet.SetBit(TagTypeInfo<T>.TagIndex);
        return new Tags(bitSet);
    }
    
    public static Tags Get<T1, T2>()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        Vector256Long bitSet = default;
        bitSet.SetBit(TagTypeInfo<T1>.TagIndex);
        bitSet.SetBit(TagTypeInfo<T2>.TagIndex);
        return new Tags(bitSet);
    }
}


