// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

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
        var structs         = new ArchetypeStructs();
        var structIndex1    = StructHeap<T>.StructIndex;
        
        structs.bitSet.SetBit(structIndex1);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes   = new StructIndexes(1,
            T1: schema.GetStructType(structIndex1, typeof(T)).structIndex
        );
        return new Signature<T>(structs, indexes);
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
        var structs         = new ArchetypeStructs();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        
        structs.bitSet.SetBit(structIndex1);
        structs.bitSet.SetBit(structIndex2);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes   = new StructIndexes(2,
            T1: schema.GetStructType(structIndex1, typeof(T1)).structIndex,
            T2: schema.GetStructType(structIndex2, typeof(T2)).structIndex
        );
        return new Signature<T1, T2>(structs, indexes);
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
        var structs         = new ArchetypeStructs();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        var structIndex3    = StructHeap<T3>.StructIndex;
        
        structs.bitSet.SetBit(structIndex1);
        structs.bitSet.SetBit(structIndex2);
        structs.bitSet.SetBit(structIndex3);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes   = new StructIndexes(3,
            T1: schema.GetStructType(structIndex1, typeof(T1)).structIndex,
            T2: schema.GetStructType(structIndex2, typeof(T2)).structIndex,
            T3: schema.GetStructType(structIndex3, typeof(T3)).structIndex
        );
        return new Signature<T1, T2, T3>(structs, indexes);
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
        var structs         = new ArchetypeStructs();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        var structIndex3    = StructHeap<T3>.StructIndex;
        var structIndex4    = StructHeap<T4>.StructIndex;
        
        structs.bitSet.SetBit(structIndex1);
        structs.bitSet.SetBit(structIndex2);
        structs.bitSet.SetBit(structIndex3);
        structs.bitSet.SetBit(structIndex4);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes   = new StructIndexes(4,
            T1: schema.GetStructType(structIndex1, typeof(T1)).structIndex,
            T2: schema.GetStructType(structIndex2, typeof(T2)).structIndex,
            T3: schema.GetStructType(structIndex3, typeof(T3)).structIndex,
            T4: schema.GetStructType(structIndex4, typeof(T4)).structIndex
        );
        return new Signature<T1, T2, T3, T4>(structs, indexes);
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
        var structs         = new ArchetypeStructs();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        var structIndex3    = StructHeap<T3>.StructIndex;
        var structIndex4    = StructHeap<T4>.StructIndex;
        var structIndex5    = StructHeap<T5>.StructIndex;
        
        structs.bitSet.SetBit(structIndex1);
        structs.bitSet.SetBit(structIndex2);
        structs.bitSet.SetBit(structIndex3);
        structs.bitSet.SetBit(structIndex4);
        structs.bitSet.SetBit(structIndex5);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var indexes   = new StructIndexes(5,
            T1: schema.GetStructType(structIndex1, typeof(T1)).structIndex,
            T2: schema.GetStructType(structIndex2, typeof(T2)).structIndex,
            T3: schema.GetStructType(structIndex3, typeof(T3)).structIndex,
            T4: schema.GetStructType(structIndex4, typeof(T4)).structIndex,
            T5: schema.GetStructType(structIndex5, typeof(T5)).structIndex
        );
        return new Signature<T1, T2, T3, T4, T5>(structs, indexes);
    }
}
#endregion

// ------------------------------------ generic Signature<> types ------------------------------------
#region generic Signature<> types

public readonly struct Signature<T>
    where T : struct, IStructComponent
{
    [Browse(Never)] public              int                 StructCount => structIndexes.length;
                    public   readonly   ArchetypeStructs    structs;
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;

    public override string ToString() => structIndexes.GetString("Signature: ");

    internal Signature(in ArchetypeStructs structs, in StructIndexes structIndexes) {
        this.structs        = structs;
        this.structIndexes  = structIndexes;
    }
}

public readonly struct Signature<T1, T2>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
    [Browse(Never)] public              int                 StructCount => structIndexes.length;
                    public   readonly   ArchetypeStructs    structs;
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;

    
    public override string ToString() => structIndexes.GetString("Signature: ");
    
    internal Signature(in ArchetypeStructs structs, in StructIndexes structIndexes) {
        this.structs        = structs;
        this.structIndexes  = structIndexes;
    }
}

public readonly struct Signature<T1, T2, T3>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
{
    [Browse(Never)] public              int                 StructCount => structIndexes.length;
                    public   readonly   ArchetypeStructs    structs;
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;
    
    public override string ToString() => structIndexes.GetString("Signature: ");
    
    internal Signature(in ArchetypeStructs structs, in StructIndexes structIndexes) {
        this.structs        = structs;
        this.structIndexes  = structIndexes;
    }
}

public readonly struct Signature<T1, T2, T3, T4>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
{
    [Browse(Never)] public              int                 StructCount => structIndexes.length;
                    public   readonly   ArchetypeStructs    structs;
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;
    
    public override string ToString() => structIndexes.GetString("Signature: ");
    
    internal Signature(in ArchetypeStructs structs, in StructIndexes structIndexes) {
        this.structs        = structs;
        this.structIndexes  = structIndexes;
    }
}

public readonly struct Signature<T1, T2, T3, T4, T5>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
    where T5 : struct, IStructComponent
{
    [Browse(Never)] public              int                 StructCount => structIndexes.length;
                    public   readonly   ArchetypeStructs    structs;
    [Browse(Never)] internal readonly   StructIndexes       structIndexes;

    public override string ToString() => structIndexes.GetString("Signature: ");

    internal Signature(in ArchetypeStructs structs, in StructIndexes structIndexes) {
        this.structs        = structs;
        this.structIndexes  = structIndexes;
    }
}

#endregion
