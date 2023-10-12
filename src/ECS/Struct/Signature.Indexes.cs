// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// ReSharper disable InconsistentNaming
/// <summary>
/// Note: The order of struct type indices matters.<br/>
/// The struct is used when dealing with generic types like: T1, T2, T3, ...   
/// </summary>
internal readonly struct SignatureIndexes
{
    internal readonly   int     length; // 4
    
    internal readonly   int     T1;     // 4
    internal readonly   int     T2;     // 4
    internal readonly   int     T3;     // 4
    internal readonly   int     T4;     // 4
    internal readonly   int     T5;     // 4
    
    // public   StructIndexesEnumerator GetEnumerator() => new (this);
    
    public override     string          ToString()      => GetString("StructIndexes: ");
    
    internal SignatureIndexes (
        int length,
        int T1  = 0,
        int T2  = 0,
        int T3  = 0,
        int T4  = 0,
        int T5  = 0
    ) {
        this.length = length;
        this.T1     = T1;
        this.T2     = T2;
        this.T3     = T3;
        this.T4     = T4;
        this.T5     = T5;
    }
    
    internal int GetIndex(int index) {
        switch (index) {
            case 0:     return T1;
            case 1:     return T2;
            case 2:     return T3;
            case 3:     return T4;
            case 4:     return T5;
            default:    throw new InvalidOperationException($"invalid index: {index}");
        }
    }
    
    internal ArchetypeStructs AsArchetypeStructs()
    {
        var structs = new ArchetypeStructs();
        switch (length) {
            case 0: goto Type0;
            case 1: goto Type1;
            case 2: goto Type2;
            case 3: goto Type3;
            case 4: goto Type4;
            case 5: goto Type5;
            default: throw new InvalidOperationException($"invalid index: {length}");
        }
        Type5:   structs.bitSet.SetBit(T5);
        Type4:   structs.bitSet.SetBit(T4);
        Type3:   structs.bitSet.SetBit(T3);
        Type2:   structs.bitSet.SetBit(T2);
        Type1:   structs.bitSet.SetBit(T1);
        Type0:   return structs;
    }
    
    internal string GetString (string prefix) {
        if (length == 0) {
            if (prefix != null) {
                return $"{prefix}[]";
            }
            return "[]";
        }
        var sb = new StringBuilder();
        if (prefix != null) {
            sb.Append(prefix);
        }
        sb.Append('[');
        var schema = EntityStore.Static.ComponentSchema;
        for (int n = 0; n < length; n++)
        {
            var index = GetIndex(n);
            sb.Append(schema.Structs[index].type.Name);
            sb.Append(", "); 
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}

/*
public struct StructIndexesEnumerator
{
    private readonly    StructIndexes   indexes;
    private             int             index;
    
    internal StructIndexesEnumerator(in StructIndexes indexes)
    {
        this.indexes    = indexes;
        index           = -1;
    }
    
    public ComponentType Current {
        get {
            var schema = EntityStore.Static.ComponentSchema;
            return schema.structs[indexes.GetIndex(index)];
        }
    }

    // --- IEnumerator
    public bool MoveNext() {
        if (index < indexes.length - 1) {
            index++;
            return true;
        }
        return false;
    }
} */


