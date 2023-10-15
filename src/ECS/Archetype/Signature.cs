// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UnusedTypeParameter
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

// ------------------------------------ generic Signature<> creation ------------------------------------
#region generic Signature<> creation

/// <summary>
/// A <see cref="Signature"/> is used to:<br/>
/// <list type="bullet">
///   <item>Get a specific <see cref="Archetype"/> of an <see cref="EntityStore"/></item>
///   <item>Create a query to process all entities of an <see cref="EntityStore"/></item>
/// </list> 
/// </summary>
// Could be a readonly struct
public static class Signature
{
    /// <summary>
    /// Returns a <see cref="Signature{T1}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore"/>.Query() methods.</item>
    /// </list> 
    /// </summary>
    public static Signature<T> Get<T>()
        where T : struct, IStructComponent
    {
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes   = new SignatureIndexes(1,
            T1: schema.CheckStructIndex(StructHeap<T>.StructIndex, typeof(T))
        );
        return new Signature<T>(indexes);
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore"/>.Query() methods.</item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2> Get<T1, T2>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes = new SignatureIndexes(2,
            T1: schema.CheckStructIndex(StructHeap<T1>.StructIndex, typeof(T1)),
            T2: schema.CheckStructIndex(StructHeap<T2>.StructIndex, typeof(T2))
        );
        return new Signature<T1, T2>(indexes);
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2,T3}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2,T3}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2,T3}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types <see cref="EntityStore"/>.Query() methods.</item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2, T3> Get<T1, T2, T3>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
    {
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes = new SignatureIndexes(3,
            T1: schema.CheckStructIndex(StructHeap<T1>.StructIndex, typeof(T1)),
            T2: schema.CheckStructIndex(StructHeap<T2>.StructIndex, typeof(T2)),
            T3: schema.CheckStructIndex(StructHeap<T3>.StructIndex, typeof(T3))
        );
        return new Signature<T1, T2, T3>(indexes);
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2,T3,T4}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2,T3,T4}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2,T3,T4}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types <see cref="EntityStore"/>.Query() methods.</item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2, T3, T4> Get<T1, T2, T3, T4>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
        where T4 : struct, IStructComponent
    {
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes = new SignatureIndexes(4,
            T1: schema.CheckStructIndex(StructHeap<T1>.StructIndex, typeof(T1)),
            T2: schema.CheckStructIndex(StructHeap<T2>.StructIndex, typeof(T2)),
            T3: schema.CheckStructIndex(StructHeap<T3>.StructIndex, typeof(T3)),
            T4: schema.CheckStructIndex(StructHeap<T4>.StructIndex, typeof(T4))
        );
        return new Signature<T1, T2, T3, T4>(indexes);
    }
    
    /// <summary>
    /// Returns a <see cref="Signature{T1,T2,T3,T4,T5}"/> containing the given struct component types.<br/>
    /// <see cref="Signature{T1,T2,T3,T4,T5}"/> features:
    /// <list type="bullet">
    ///   <item>Get the <see cref="Archetype"/> of an <see cref="EntityStore"/> using <see cref="EntityStore.GetArchetype{T1,T2,T3,T4,T5}"/>.</item>
    ///   <item>Create a query to process all entities containing the given struct component types <see cref="EntityStore"/>.Query() methods.</item>
    /// </list> 
    /// </summary>
    public static Signature<T1, T2, T3, T4, T5> Get<T1, T2, T3, T4, T5>()
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
        where T4 : struct, IStructComponent
        where T5 : struct, IStructComponent
    {
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes = new SignatureIndexes(5,
            T1: schema.CheckStructIndex(StructHeap<T1>.StructIndex, typeof(T1)),
            T2: schema.CheckStructIndex(StructHeap<T2>.StructIndex, typeof(T2)),
            T3: schema.CheckStructIndex(StructHeap<T3>.StructIndex, typeof(T3)),
            T4: schema.CheckStructIndex(StructHeap<T4>.StructIndex, typeof(T4)),
            T5: schema.CheckStructIndex(StructHeap<T5>.StructIndex, typeof(T5))
        );
        return new Signature<T1, T2, T3, T4, T5>(indexes);
    }
}
#endregion

// ------------------------------------ generic Signature<> types ------------------------------------
#region generic Signature<> types

public readonly struct Signature<T>
    where T : struct, IStructComponent
{
    public                              ArchetypeStructs    Structs     => new ArchetypeStructs(signatureIndexes);
    [Browse(Never)] public              int                 StructCount => signatureIndexes.length;
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 32

    public override string ToString() => signatureIndexes.GetString("Signature: ");

    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes  = signatureIndexes;
    }
}

public readonly struct Signature<T1, T2>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    public                              ArchetypeStructs    Structs     => new ArchetypeStructs(signatureIndexes);
    [Browse(Never)] public              int                 StructCount => signatureIndexes.length;
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 32
    
    public override string ToString() => signatureIndexes.GetString("Signature: ");
    
    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes  = signatureIndexes;
    }
}

public readonly struct Signature<T1, T2, T3>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
{
    public                              ArchetypeStructs    Structs     => new ArchetypeStructs(signatureIndexes);
    [Browse(Never)] public              int                 StructCount => signatureIndexes.length;
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 32
    
    
    public override string ToString() => signatureIndexes.GetString("Signature: ");
    
    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes = signatureIndexes;
    }
}

public readonly struct Signature<T1, T2, T3, T4>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
{
    public                              ArchetypeStructs    Structs     => new ArchetypeStructs(signatureIndexes);
    [Browse(Never)] public              int                 StructCount => signatureIndexes.length;
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 32
    
    public override string ToString() => signatureIndexes.GetString("Signature: ");
    
    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes = signatureIndexes;
    }
}

public readonly struct Signature<T1, T2, T3, T4, T5>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
    where T5 : struct, IStructComponent
{
    public                              ArchetypeStructs    Structs     => new ArchetypeStructs(signatureIndexes);
    [Browse(Never)] public              int                 StructCount => signatureIndexes.length;
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 32

    public override string ToString() => signatureIndexes.GetString("Signature: ");

    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes = signatureIndexes;
    }
}

#endregion
