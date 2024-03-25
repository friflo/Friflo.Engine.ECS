// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace Friflo.Engine.ECS;

/// <summary>
/// Note: The order of struct type indices matters.<br/>
/// The struct is used when dealing with generic types like: T1, T2, T3, ...   
/// </summary>
internal readonly struct SignatureIndexes
{
    internal readonly   int     length; // 4    - values: [1, 5] ensured by assertion
    
    internal readonly   int     T1;     // 4
    internal readonly   int     T2;     // 4
    internal readonly   int     T3;     // 4
    internal readonly   int     T4;     // 4
    internal readonly   int     T5;     // 4
    
    // public   StructIndexesEnumerator GetEnumerator() => new (this);
    
    public override     string          ToString()      => GetString("SignatureIndexes: ");
    
    internal SignatureIndexes (
        int length,
        int T1  = 0,
        int T2  = 0,
        int T3  = 0,
        int T4  = 0,
        int T5  = 0
    ) {
        AssertLength(length);
        this.length = length;
        this.T1     = T1;
        this.T2     = T2;
        this.T3     = T3;
        this.T4     = T4;
        this.T5     = T5;
    }
    
    [ExcludeFromCodeCoverage]
    private static void AssertLength(int length) {
        if (length is < 1 or > 5) {
            throw new IndexOutOfRangeException();
        }
    }
    
    internal readonly int GetStructIndex(int index) {
        if (index >= length) {
            throw new IndexOutOfRangeException();
        }
        switch (index) {
            case 0:     return T1;
            case 1:     return T2;
            case 2:     return T3;
            case 3:     return T4;
            case 4:     return T5;
        //  default:    throw new IndexOutOfRangeException(); // unreachable - already ensured by constructor
        }
        return -1;  // unreachable - all valid cases are covered
    }
    
    internal readonly string GetString (string prefix) {
        var sb = new StringBuilder();
        if (prefix != null) {
            sb.Append(prefix);
        }
        sb.Append('[');
        var components = EntityStoreBase.Static.EntitySchema.components;
        for (int n = 0; n < length; n++)
        {
            var structIndex = GetStructIndex(n);
            sb.Append(components[structIndex].Name);
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
            var schema = EntityStore.Static.EntitySchema;
            return schema.componentTypes[indexes.GetIndex(index)];
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


