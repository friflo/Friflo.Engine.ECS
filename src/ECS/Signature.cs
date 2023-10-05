// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType
// ReSharper disable ArrangeTrailingCommaInMultilineLists
namespace Friflo.Fliox.Engine.ECS;

// Could be a readonly struct
public sealed class Signature
{
    // --- public fields
    public  readonly    int             signatureIndex;
    public  readonly    long            hash;
    public  ReadOnlySpan<ComponentType> ComponentTypes => componentTypes;
    
    // --- private fields
    private readonly    ComponentType[] componentTypes;
    
    // --- static
    private static readonly Dictionary<long, Signature> Signatures = new Dictionary<long, Signature>();
    private static          int                         NextSignatureIndex;
    
    
    internal Signature(ComponentType[] componentTypes, int signatureIndex, long hash)
    {
        this.componentTypes = componentTypes;
        this.signatureIndex = signatureIndex;
        this.hash           = hash;
    }
    
    private static Signature Create(ComponentType[] componentTypes, long hash)
    {
        var signatureIndex  = NextSignatureIndex++; // todo check max signatures
        var signature       = new Signature(componentTypes, signatureIndex, hash);
        Signatures.Add(hash, signature);
        return signature;
    }
    
    public static Signature Create<T>()
        where T : struct
    {
        var hash = typeof(T).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T>.StructIndex, compTypes.Structs.Length, typeof(T))
        };
        return Create(types, hash);
    }
    
    public static Signature Create<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, compTypes.Structs.Length, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, compTypes.Structs.Length, typeof(T2)),
        };
        return Create(types, hash);
    }
}

