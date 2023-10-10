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
    ///   <item>Create a query to process all entities containing the given struct component types with <see cref="EntityStore.Query{T1}"/></item>
    /// </list> 
    /// </summary>
    public static Signature<T> Get<T>()
        where T : struct, IStructComponent
    {
        var mask            = new ArchetypeMask();
        var structIndex1    = StructHeap<T>.StructIndex;
        
        mask.bitSet.SetBit(structIndex1);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var types   = new SignatureTypeSet(1,
            T1: schema.GetStructType(structIndex1, typeof(T))
        );
        return new Signature<T>(mask, types);
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
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        var mask            = new ArchetypeMask();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        
        mask.bitSet.SetBit(structIndex1);
        mask.bitSet.SetBit(structIndex2);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var types   = new SignatureTypeSet(2,
            T1: schema.GetStructType(structIndex1, typeof(T1)),
            T2: schema.GetStructType(structIndex2, typeof(T2))
        );
        return new Signature<T1, T2>(mask, types);
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
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
    {
        var mask            = new ArchetypeMask();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        var structIndex3    = StructHeap<T3>.StructIndex;
        
        mask.bitSet.SetBit(structIndex1);
        mask.bitSet.SetBit(structIndex2);
        mask.bitSet.SetBit(structIndex3);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var types   = new SignatureTypeSet(3,
            T1: schema.GetStructType(structIndex1, typeof(T1)),
            T2: schema.GetStructType(structIndex2, typeof(T2)),
            T3: schema.GetStructType(structIndex3, typeof(T3))
        );
        return new Signature<T1, T2, T3>(mask, types);
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
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
        where T4 : struct, IStructComponent
    {
        var mask            = new ArchetypeMask();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        var structIndex3    = StructHeap<T3>.StructIndex;
        var structIndex4    = StructHeap<T4>.StructIndex;
        
        mask.bitSet.SetBit(structIndex1);
        mask.bitSet.SetBit(structIndex2);
        mask.bitSet.SetBit(structIndex3);
        mask.bitSet.SetBit(structIndex4);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var types   = new SignatureTypeSet(4,
            T1: schema.GetStructType(structIndex1, typeof(T1)),
            T2: schema.GetStructType(structIndex2, typeof(T2)),
            T3: schema.GetStructType(structIndex3, typeof(T3)),
            T4: schema.GetStructType(structIndex4, typeof(T4))
        );
        return new Signature<T1, T2, T3, T4>(mask, types);
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
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
        where T4 : struct, IStructComponent
        where T5 : struct, IStructComponent
    {
        var mask            = new ArchetypeMask();
        var structIndex1    = StructHeap<T1>.StructIndex;
        var structIndex2    = StructHeap<T2>.StructIndex;
        var structIndex3    = StructHeap<T3>.StructIndex;
        var structIndex4    = StructHeap<T4>.StructIndex;
        var structIndex5    = StructHeap<T5>.StructIndex;
        
        mask.bitSet.SetBit(structIndex1);
        mask.bitSet.SetBit(structIndex2);
        mask.bitSet.SetBit(structIndex3);
        mask.bitSet.SetBit(structIndex4);
        mask.bitSet.SetBit(structIndex5);
        
        var schema  = EntityStore.Static.ComponentSchema;
        var types   = new SignatureTypeSet(5,
            T1: schema.GetStructType(structIndex1, typeof(T1)),
            T2: schema.GetStructType(structIndex2, typeof(T2)),
            T3: schema.GetStructType(structIndex3, typeof(T3)),
            T4: schema.GetStructType(structIndex4, typeof(T4)),
            T5: schema.GetStructType(structIndex5, typeof(T5))
            
        );
        return new Signature<T1, T2, T3, T4, T5>(mask, types);
    }
}
#endregion

// ------------------------------------ generic Signature<> types ------------------------------------
#region generic Signature<> types

public readonly struct Signature<T>
    where T : struct, IStructComponent
{
                    public   readonly   SignatureTypeSet    types;
    [Browse(Never)] public   readonly   ArchetypeMask       mask;

    public override string ToString() => types.GetString("Signature: ");

    internal Signature(in ArchetypeMask mask, in SignatureTypeSet types) {
        this.mask   = mask;
        this.types  = types;
    }
}

public readonly struct Signature<T1, T2>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
{
                    public   readonly   SignatureTypeSet    types;
    [Browse(Never)] public   readonly   ArchetypeMask       mask;
    
    public override string ToString() => types.GetString("Signature: ");
    
    internal Signature(in ArchetypeMask mask, in SignatureTypeSet types) {
        this.mask   = mask;
        this.types  = types;
    }
}

public readonly struct Signature<T1, T2, T3>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
{
                    public   readonly   SignatureTypeSet    types;
    [Browse(Never)] public   readonly   ArchetypeMask       mask;
    
    public override string ToString() => types.GetString("Signature: ");
    
    internal Signature(in ArchetypeMask mask, in SignatureTypeSet types) {
        this.mask   = mask;
        this.types  = types;
    }
}

public readonly struct Signature<T1, T2, T3, T4>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
{
                    public   readonly   SignatureTypeSet    types;
    [Browse(Never)] public   readonly   ArchetypeMask       mask;
    
    public override string ToString() => types.GetString("Signature: ");
    
    internal Signature(in ArchetypeMask mask, in SignatureTypeSet types) {
        this.mask   = mask;
        this.types  = types;
    }
}

public readonly struct Signature<T1, T2, T3, T4, T5>
    where T1 : struct, IStructComponent
    where T2 : struct, IStructComponent
    where T3 : struct, IStructComponent
    where T4 : struct, IStructComponent
    where T5 : struct, IStructComponent
{
                    public   readonly   SignatureTypeSet    types;
    [Browse(Never)] public   readonly   ArchetypeMask       mask;

    public override string ToString() => types.GetString("Signature: ");

    internal Signature(in ArchetypeMask mask, in SignatureTypeSet types) {
        this.mask   = mask;
        this.types  = types;
    }
}

#endregion
