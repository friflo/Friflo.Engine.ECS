// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType
// ReSharper disable ArrangeTrailingCommaInMultilineLists
namespace Friflo.Fliox.Engine.ECS;

// Could be a readonly struct
public class Signature
{
    // --- public fields
    public   readonly   int                 index;
    public   readonly   long                hash;
    public   ReadOnlySpan<ComponentType>    ComponentTypes => componentTypes;
    
    public   override   string              ToString() => GetString();

    // --- private fields
    internal readonly   ComponentType[]     componentTypes;
    
    // --- static
    private static readonly Dictionary<long, Signature> Signatures = new Dictionary<long, Signature>();
    private static          int                         NextSignatureIndex;
    
    
    internal Signature(ComponentType[] componentTypes, int index, long hash)
    {
        this.componentTypes = componentTypes;
        this.index          = index;
        this.hash           = hash;
    }
    
    private static int NextIndex()
    {
        return NextSignatureIndex++; // todo check max signatures
    }
    
    private string GetString() {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var type in componentTypes) {
            sb.Append(type.type.Name);
            sb.Append(", ");
        }
        sb.Length -= 2;
        sb.Append(']');
        return sb.ToString();
    }
    
    public static Signature<T> Get<T>()
        where T : struct
    {
        var hash = typeof(T).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T>.StructIndex, compTypes.Structs.Length, typeof(T))
        };
        var signature       = new Signature<T>(types, NextIndex(), hash);
        Signatures.Add(hash, signature);
        return signature;
    }
    
    public static Signature<T1, T2> Get<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1,T2>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, compTypes.Structs.Length, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, compTypes.Structs.Length, typeof(T2)),
        };
        var signature       = new Signature<T1, T2>(types, NextIndex(), hash);
        Signatures.Add(hash, signature);
        return signature;
    }
    
    public static Signature<T1, T2, T3> Get<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1, T2, T3>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, compTypes.Structs.Length, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, compTypes.Structs.Length, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, compTypes.Structs.Length, typeof(T3)),
        };
        var signature       = new Signature<T1, T2, T3>(types, NextIndex(), hash);
        Signatures.Add(hash, signature);
        return signature;
    }
    
    public static Signature<T1, T2, T3, T4> Get<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle() ^
                   typeof(T4).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1, T2, T3, T4>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, compTypes.Structs.Length, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, compTypes.Structs.Length, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, compTypes.Structs.Length, typeof(T3)),
            compTypes.GetStructType(StructHeap<T4>.StructIndex, compTypes.Structs.Length, typeof(T4)),
        };
        var signature       = new Signature<T1, T2, T3, T4>(types, NextIndex(), hash);
        Signatures.Add(hash, signature);
        return signature;
    }
    
    public static Signature<T1, T2, T3, T4, T5> Get<T1, T2, T3, T4, T5>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle() ^
                   typeof(T3).Handle() ^
                   typeof(T4).Handle() ^
                   typeof(T5).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1, T2, T3, T4, T5>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, compTypes.Structs.Length, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, compTypes.Structs.Length, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, compTypes.Structs.Length, typeof(T3)),
            compTypes.GetStructType(StructHeap<T4>.StructIndex, compTypes.Structs.Length, typeof(T4)),
            compTypes.GetStructType(StructHeap<T5>.StructIndex, compTypes.Structs.Length, typeof(T5)),
        };
        var signature       = new Signature<T1, T2, T3, T4, T5>(types, NextIndex(), hash);
        Signatures.Add(hash, signature);
        return signature;
    }
}


public class Signature<T> : Signature
    where T : struct
{
    internal Signature(ComponentType[] componentTypes, int index, long hash)
        : base(componentTypes, index, hash) {
    }
}

public class Signature<T1, T2> : Signature
    where T1 : struct
    where T2 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex, long hash)
        : base(componentTypes, signatureIndex, hash) {
    }
}

public class Signature<T1, T2, T3> : Signature
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex, long hash)
        : base(componentTypes, signatureIndex, hash) {
    }
}

public class Signature<T1, T2, T3, T4> : Signature
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex, long hash)
        : base(componentTypes, signatureIndex, hash) {
    }
}

public class Signature<T1, T2, T3, T4, T5> : Signature
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex, long hash)
        : base(componentTypes, signatureIndex, hash) {
    }
}


