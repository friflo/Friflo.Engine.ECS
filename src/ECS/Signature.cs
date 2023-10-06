// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using static System.Numerics.BitOperations;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType
// ReSharper disable ArrangeTrailingCommaInMultilineLists
namespace Friflo.Fliox.Engine.ECS;

/// <summary>
/// A <see cref="Signature"/> contains a set of struct <see cref="ComponentTypes"/>.<br/>
/// <see cref="Signature"/> features:
/// <list type="bullet">
///   <item>Get a specific <see cref="Archetype"/> of an <see cref="EntityStore"/></item>
///   <item>Create a query to process all entities of an <see cref="EntityStore"/> owning the struct <see cref="ComponentTypes"/> </item>
/// </list> 
/// </summary>
// Could be a readonly struct
public abstract class Signature
{
    // --- public fields
                    public   readonly   int                 index;
    /// <summary>Note: different order of same generic <see cref="Signature"/> arguments result in a different hash</summary>
                    public   readonly   long                archetypeHash;
                    public   ReadOnlySpan<ComponentType>    ComponentTypes => componentTypes;
    
                    public   override   string              ToString() => GetString();

    // --- private fields
    [Browse(Never)] internal readonly   ComponentType[]     componentTypes;
    
    // --- static
    private static readonly Dictionary<ulong, Signature>    Signatures = new Dictionary<ulong, Signature>();
    private static          int                             NextSignatureIndex;
    
    
    internal Signature(ComponentType[] componentTypes, int index)
    {
        this.componentTypes = componentTypes;
        this.index          = index;
        long hash = 0;
        foreach (var type in componentTypes) {
            hash ^= type.type.Handle();
        }
        archetypeHash = hash;
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
    
    /// <summary>
    /// Returns a <see cref="Signature{T1}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore.Query{T1}"/></item>
    /// </list> 
    /// </summary>
    public static Signature<T> Get<T>()
        where T : struct
    {
        var hash = typeof(T).HandleUInt64();
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T>.StructIndex, typeof(T))
        };
        var signature   = new Signature<T>(types, NextIndex());
        Signatures.Add(hash, signature);
        return signature;
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore.Query{T1,T2}"/></item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2> Get<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        var hash1   = typeof(T1).HandleUInt64();
        var hash2   = RotateLeft(typeof(T2).HandleUInt64(), 7);
        var hash    = hash1 ^ hash2;
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1,T2>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, typeof(T2)),
        };
        var signature   = new Signature<T1, T2>(types, NextIndex());
        Signatures.Add(hash, signature);
        return signature;
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2,T3}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2,T3}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2,T3}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore.Query{T1,T2,T3}"/></item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2, T3> Get<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var hash1   = typeof(T1).HandleUInt64();
        var hash2   = RotateLeft(typeof(T2).HandleUInt64(), 7);
        var hash3   = RotateLeft(typeof(T3).HandleUInt64(), 14);
        var hash    = hash1 ^ hash2 ^ hash3;
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1, T2, T3>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, typeof(T3)),
        };
        var signature   = new Signature<T1, T2, T3>(types, NextIndex());
        Signatures.Add(hash, signature);
        return signature;
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2,T3,T4}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2,T3,T4}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2,T3,T4}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore.Query{T1,T2,T3,T4}"/></item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2, T3, T4> Get<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var hash1   = typeof(T1).HandleUInt64();
        var hash2   = RotateLeft(typeof(T2).HandleUInt64(), 7);
        var hash3   = RotateLeft(typeof(T3).HandleUInt64(), 14);
        var hash4   = RotateLeft(typeof(T4).HandleUInt64(), 21);
        var hash    = hash1 ^ hash2 ^ hash3 ^ hash4;
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1, T2, T3, T4>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, typeof(T3)),
            compTypes.GetStructType(StructHeap<T4>.StructIndex, typeof(T4)),
        };
        var signature   = new Signature<T1, T2, T3, T4>(types, NextIndex());
        Signatures.Add(hash, signature);
        return signature;
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2,T3,T4,T5}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2,T3,T4,T5}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2,T3,T4,T5}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore.Query{T1,T2,T3,T4,T5}"/></item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2, T3, T4, T5> Get<T1, T2, T3, T4, T5>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
    {
        var hash1   = typeof(T1).HandleUInt64();
        var hash2   = RotateLeft(typeof(T2).HandleUInt64(), 7);
        var hash3   = RotateLeft(typeof(T3).HandleUInt64(), 14);
        var hash4   = RotateLeft(typeof(T4).HandleUInt64(), 21);
        var hash5   = RotateLeft(typeof(T5).HandleUInt64(), 28);
        var hash    = hash1 ^ hash2 ^ hash3 ^ hash4 ^ hash5;
        
        if (Signatures.TryGetValue(hash, out var result)) {
            return (Signature<T1, T2, T3, T4, T5>)result;
        }
        var compTypes   = EntityStore.Static.ComponentTypes;
        var types       = new [] {
            compTypes.GetStructType(StructHeap<T1>.StructIndex, typeof(T1)),
            compTypes.GetStructType(StructHeap<T2>.StructIndex, typeof(T2)),
            compTypes.GetStructType(StructHeap<T3>.StructIndex, typeof(T3)),
            compTypes.GetStructType(StructHeap<T4>.StructIndex, typeof(T4)),
            compTypes.GetStructType(StructHeap<T5>.StructIndex, typeof(T5)),
        };
        var signature = new Signature<T1, T2, T3, T4, T5>(types, NextIndex());
        Signatures.Add(hash, signature);
        return signature;
    }
}


public sealed class Signature<T> : Signature
    where T : struct
{
    internal Signature(ComponentType[] componentTypes, int index)
        : base(componentTypes, index) {
    }
}

public sealed class Signature<T1, T2> : Signature
    where T1 : struct
    where T2 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex)
        : base(componentTypes, signatureIndex) {
    }
}

public sealed class Signature<T1, T2, T3> : Signature
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex)
        : base(componentTypes, signatureIndex) {
    }
}

public sealed class Signature<T1, T2, T3, T4> : Signature
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex)
        : base(componentTypes, signatureIndex) {
    }
}

public sealed class Signature<T1, T2, T3, T4, T5> : Signature
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
{
    internal Signature(ComponentType[] componentTypes, int signatureIndex)
        : base(componentTypes, signatureIndex) {
    }
}


