// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UnusedTypeParameter
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

// ------------------------------------ generic Signature<> creation ------------------------------------
#region generic Signature<> creation

/// <summary>
/// A <see cref="Signature"/> specify the <see cref="IComponent"/> types used to query entity components<br/>
/// using the <see cref="EntityStore"/>.Query(<see cref="Signature"/>) methods.
/// </summary>
/// <remarks>
/// In contrast to <see cref="ComponentTypes"/> the order of <see cref="IComponent"/>'s stored in a signature is
/// relevant for queries.<br/>
/// The maximum number of <see cref="IComponent"/>'s stored in a signature is currently 5.<br/>
/// </remarks>
[CLSCompliant(true)]
public static class Signature
{
    /// <summary>
    /// Returns a query <see cref="Signature{T1}"/> containing the specified component type.<br/>
    /// </summary>
    /// <remarks>
    /// It can be used to query entities with the specified component type with <see cref="EntityStore"/>.Query() methods.
    /// </remarks>
    public static Signature<T1> Get<T1>()
        where T1 : struct, IComponent
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        var indexes   = new SignatureIndexes(
            T1: schema.CheckStructIndex(typeof(T1), StructInfo<T1>.Index)
        );
        return new Signature<T1>(indexes);
    }
    
    /// <summary>
    /// Returns a query <see cref="Signature{T1, T2}"/> containing the specified component types.<br/>
    /// </summary>
    /// <remarks>
    /// It can be used to query entities with the specified component types with <see cref="EntityStore"/>.Query() methods.
    /// </remarks>
    public static Signature<T1, T2> Get<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        var indexes = new SignatureIndexes(
            T1: schema.CheckStructIndex(typeof(T1), StructInfo<T1>.Index),
            T2: schema.CheckStructIndex(typeof(T2), StructInfo<T2>.Index)
        );
        return new Signature<T1, T2>(indexes);
    }
    
    /// <summary>
    /// Returns a query <see cref="Signature{T1,T2,T3}"/> containing the specified component types.<br/>
    /// </summary>
    /// <remarks>
    /// It can be used to query entities with the specified component types with <see cref="EntityStore"/>.Query() methods.
    /// </remarks>
    public static Signature<T1, T2, T3> Get<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        var indexes = new SignatureIndexes(
            T1: schema.CheckStructIndex(typeof(T1), StructInfo<T1>.Index),
            T2: schema.CheckStructIndex(typeof(T2), StructInfo<T2>.Index),
            T3: schema.CheckStructIndex(typeof(T3), StructInfo<T3>.Index)
        );
        return new Signature<T1, T2, T3>(indexes);
    }
    
    /// <summary>
    /// Returns a query <see cref="Signature{T1,T2,T3,T4}"/> containing the specified component types.<br/>
    /// </summary>
    /// <remarks>
    /// It can be used to query entities with the specified component types with <see cref="EntityStore"/>.Query() methods.
    /// </remarks>
    public static Signature<T1, T2, T3, T4> Get<T1, T2, T3, T4>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        var indexes = new SignatureIndexes(
            T1: schema.CheckStructIndex(typeof(T1), StructInfo<T1>.Index),
            T2: schema.CheckStructIndex(typeof(T2), StructInfo<T2>.Index),
            T3: schema.CheckStructIndex(typeof(T3), StructInfo<T3>.Index),
            T4: schema.CheckStructIndex(typeof(T4), StructInfo<T4>.Index)
        );
        return new Signature<T1, T2, T3, T4>(indexes);
    }
    
    /// <summary>
    /// Returns a query <see cref="Signature{T1,T2,T3,T4,T5}"/> containing the specified component types.<br/>
    /// </summary>
    /// <remarks>
    /// It can be used to query entities with the specified component types with <see cref="EntityStore"/>.Query() methods.
    /// </remarks>
    public static Signature<T1, T2, T3, T4, T5> Get<T1, T2, T3, T4, T5>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        var schema  = EntityStoreBase.Static.EntitySchema;
        var indexes = new SignatureIndexes(
            T1: schema.CheckStructIndex(typeof(T1), StructInfo<T1>.Index),
            T2: schema.CheckStructIndex(typeof(T2), StructInfo<T2>.Index),
            T3: schema.CheckStructIndex(typeof(T3), StructInfo<T3>.Index),
            T4: schema.CheckStructIndex(typeof(T4), StructInfo<T4>.Index),
            T5: schema.CheckStructIndex(typeof(T5), StructInfo<T5>.Index)
        );
        return new Signature<T1, T2, T3, T4, T5>(indexes);
    }
    
    internal static string GetSignatureString(in SignatureIndexes indexes) {
        return indexes.GetString("Signature: ");
    }
}
#endregion

// ------------------------------------ generic Signature<> types ------------------------------------
#region generic Signature<> types

/// <summary>
/// A Signature to create a query using <see cref="EntityStoreBase.Query{T1}(Signature{T1})"/> with one component.
/// </summary>
public readonly struct Signature<T1>
    where T1 : struct, IComponent
{
    /// <summary> Return the component types of the query signature. </summary>
    public                              ComponentTypes      ComponentTypes  => new ComponentTypes(signatureIndexes);
    
    /// <summary> Gets the number component types of the query signature. </summary>
    [Browse(Never)] public              int                 ComponentCount  => signatureIndexes.length;
    
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 16

    public override string ToString() => Signature.GetSignatureString(signatureIndexes);

    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes  = signatureIndexes;
    }
}

/// <summary>
/// A Signature to create a query using <see cref="EntityStoreBase.Query{T1,T2}(Signature{T1,T2})"/> with two components.
/// </summary>
public readonly struct Signature<T1, T2>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    /// <summary> Return the component types of the query signature. </summary>
    public                              ComponentTypes      ComponentTypes  => new ComponentTypes(signatureIndexes);
    
    /// <summary> Gets the number component types of the query signature. </summary>
    [Browse(Never)] public              int                 ComponentCount  => signatureIndexes.length;
    
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 16
    
    public override string ToString() => Signature.GetSignatureString(signatureIndexes);
    
    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes  = signatureIndexes;
    }
}

/// <summary>
/// A Signature to create a query using <see cref="EntityStoreBase.Query{T1,T2,T3}(Signature{T1,T2,T3})"/> with three components.
/// </summary>
public readonly struct Signature<T1, T2, T3>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    /// <summary> Return the component types of the query signature. </summary>
    public                              ComponentTypes      ComponentTypes  => new ComponentTypes(signatureIndexes);
    
    /// <summary> Gets the number component types of the query signature. </summary>
    [Browse(Never)] public              int                 ComponentCount  => signatureIndexes.length;
    
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 16
    
    public override string ToString() => Signature.GetSignatureString(signatureIndexes);
    
    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes = signatureIndexes;
    }
}

/// <summary>
/// A Signature to create a query using <see cref="EntityStoreBase.Query{T1,T2,T3,T4}(Signature{T1,T2,T3,T4})"/> with four components.
/// </summary>
public readonly struct Signature<T1, T2, T3, T4>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{
    /// <summary> Return the component types of the query signature. </summary>
    public                              ComponentTypes      ComponentTypes  => new ComponentTypes(signatureIndexes);
    
    /// <summary> Gets the number component types of the query signature. </summary>
    [Browse(Never)] public              int                 ComponentCount  => signatureIndexes.length;
    
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 16
    
    public override string ToString() => Signature.GetSignatureString(signatureIndexes);
    
    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes = signatureIndexes;
    }
}

/// <summary>
/// A Signature used to create a query using <see cref="EntityStoreBase.Query{T1,T2,T3,T4,T5}(Signature{T1,T2,T3,T4,T5})"/> with five components.
/// </summary>
public readonly struct Signature<T1, T2, T3, T4, T5>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
{
    /// <summary> Return the component types of the query signature. </summary>
    public                              ComponentTypes      ComponentTypes  => new ComponentTypes(signatureIndexes);
    
    /// <summary> Gets the number component types of the query signature. </summary>
    [Browse(Never)] public              int                 ComponentCount  => signatureIndexes.length;
    
    [Browse(Never)] internal readonly   SignatureIndexes    signatureIndexes;   // 16

    public override string ToString() => Signature.GetSignatureString(signatureIndexes);

    internal Signature(in SignatureIndexes signatureIndexes) {
        this.signatureIndexes = signatureIndexes;
    }
}

#endregion
