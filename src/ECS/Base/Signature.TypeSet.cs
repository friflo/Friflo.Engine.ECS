// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct SignatureTypeSet
{
    internal readonly   int             length;
    internal readonly   ComponentType   T1;
    internal readonly   ComponentType   T2;
    internal readonly   ComponentType   T3;
    internal readonly   ComponentType   T4;
    internal readonly   ComponentType   T5;
    
    public              int             Length          => length;
    public   SignatureTypeSetEnumerator GetEnumerator() => new (this);
    public override     string          ToString()      => GetString();

    internal SignatureTypeSet(
        int             length,
        ComponentType   T1 = null,
        ComponentType   T2 = null,
        ComponentType   T3 = null,
        ComponentType   T4 = null,
        ComponentType   T5 = null)
    {
        if (length > 5) {
            throw new InvalidOperationException($"exceed maximum length 5. was {length}");
        }
        this.length = length;
        this.T1     = T1;
        this.T2     = T2;
        this.T3     = T3;
        this.T4     = T4;
        this.T5     = T5;
    }
    
    public ComponentType this [int index] {
        get {
            if (index >= length) {
                throw new IndexOutOfRangeException($"length: {length}, index: {index}");
            }
            switch (index) {
                case 0:     return T1;
                case 1:     return T2;
                case 2:     return T3;
                case 3:     return T4;
                case 4:     return T5;
                default:
                    throw new IndexOutOfRangeException($"length: {length}, index: {index}");
            }
        }
    }
    
    internal string GetString () {
        if (length == 0) {
            return "[]";
        }
        var sb = new StringBuilder();
        sb.Append('[');
        for (int n = 0; n < length; n++)
        {
            sb.Append(this[n].type.Name);
            sb.Append(", "); 
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
}

public struct SignatureTypeSetEnumerator
{
    private readonly    SignatureTypeSet    types;
    private             int                 index;
    
    internal SignatureTypeSetEnumerator(in SignatureTypeSet types)
    {
        this.types  = types;
        index       = -1;
    }
    
    public ComponentType Current   => types[index];
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < types.length - 1) {
            index++;
            return true;
        }
        return false;
    }
}



