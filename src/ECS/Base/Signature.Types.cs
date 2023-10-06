// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public struct SignatureTypes
{
    internal            int             length;
    internal            ComponentType   T1;
    internal            ComponentType   T2;
    internal            ComponentType   T3;
    internal            ComponentType   T4;
    internal            ComponentType   T5;
    
    public              int             Length          => length;
    public  SignatureTypesEnumerator    GetEnumerator() => new (this);
    public override     string          ToString()      => GetString();

    internal SignatureTypes(int length) {
        this.length = length;
    }
    
    public void Clear() {
        length = 0;
        T1 = null;
        T2 = null;
        T3 = null;
        T4 = null;
        T5 = null;
    }
    
    public void Add(ComponentType type) {
        this[length++] = type;
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
        set {
            if (index >= length) {
                throw new IndexOutOfRangeException($"length: {length}, index: {index}");
            }
            switch (index) {
                case 0:     T1 = value;     return;
                case 1:     T2 = value;     return;
                case 2:     T3 = value;     return;
                case 3:     T4 = value;     return;
                case 4:     T5 = value;     return;
                default:
                    throw new IndexOutOfRangeException($"length: {length}, index: {index}");
            }
        }
    }
    
    private string GetString () {
        if (length == 0) {
            return "";
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

public struct SignatureTypesEnumerator
{
    private readonly    SignatureTypes  types;
    private             int             index;
    
    internal SignatureTypesEnumerator(in SignatureTypes types)
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



