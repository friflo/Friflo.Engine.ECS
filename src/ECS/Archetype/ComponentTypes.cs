// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Friflo.Engine.ECS.Utils;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="ComponentTypes"/> define a set of <see cref="IComponent"/>'s used to list the
/// component <see cref="System.Type"/>'s of an <see cref="Archetype"/>.
/// </summary>
[CLSCompliant(true)]
[DebuggerTypeProxy(typeof(ComponentTypesDebugView))]
public struct ComponentTypes : IEnumerable<ComponentType>
{
#region public properties    
    /// <summary>Return the number of contained <see cref="IComponent"/>'s.</summary>
    public readonly int     Count       => bitSet.GetBitCount();
    public override string  ToString()  => GetString();
    #endregion
    
#region interal field
    internal        BitSet  bitSet;     // 32
    #endregion

#region enumerator    
    public readonly ComponentTypesEnumerator    GetEnumerator()                             => new ComponentTypesEnumerator (this);

    // --- IEnumerable
           readonly IEnumerator                 IEnumerable.GetEnumerator()                 => new ComponentTypesEnumerator (this);

    // --- IEnumerable<>
           readonly IEnumerator<ComponentType>  IEnumerable<ComponentType>.GetEnumerator()  => new ComponentTypesEnumerator (this);
#endregion

#region specialized constructor
    public ComponentTypes(ComponentType type)
    {
        bitSet.SetBit(type.StructIndex);
    }
    #endregion


#region internal constructors
    internal ComponentTypes(StructHeap[] heaps) {
        foreach (var heap in heaps) {
            bitSet.SetBit(heap.structIndex);
        }
    }
    
    internal ComponentTypes(in SignatureIndexes indexes)
    {
        switch (indexes.length) {
        //  case 0: return;     cannot happen: length is > 0
            case 1: goto Type1;
            case 2: goto Type2;
            case 3: goto Type3;
            case 4: goto Type4;
            case 5: goto Type5;
            default: throw new IndexOutOfRangeException(); // unreachable - already ensured at SignatureIndexes
        }
        Type5:   bitSet.SetBit(indexes.T5);
        Type4:   bitSet.SetBit(indexes.T4);
        Type3:   bitSet.SetBit(indexes.T3);
        Type2:   bitSet.SetBit(indexes.T2);
        Type1:   bitSet.SetBit(indexes.T1);
    }
    
    internal ComponentTypes(int t1)
    {
        bitSet.SetBit(t1);
    }
    
    internal ComponentTypes(int t1, int t2)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
    }
    
    
    internal ComponentTypes(int t1, int t2, int t3)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4, int t5)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
        bitSet.SetBit(t5);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4, int t5, int t6)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
        bitSet.SetBit(t5);
        bitSet.SetBit(t6);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4, int t5, int t6, int t7)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
        bitSet.SetBit(t5);
        bitSet.SetBit(t6);
        bitSet.SetBit(t7);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4, int t5, int t6, int t7, int t8)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
        bitSet.SetBit(t5);
        bitSet.SetBit(t6);
        bitSet.SetBit(t7);
        bitSet.SetBit(t8);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4, int t5, int t6, int t7, int t8, int t9)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
        bitSet.SetBit(t5);
        bitSet.SetBit(t6);
        bitSet.SetBit(t7);
        bitSet.SetBit(t8);
        bitSet.SetBit(t9);
    }
    
    internal ComponentTypes(int t1, int t2, int t3, int t4, int t5, int t6, int t7, int t8, int t9, int t10)
    {
        bitSet.SetBit(t1);
        bitSet.SetBit(t2);
        bitSet.SetBit(t3);
        bitSet.SetBit(t4);
        bitSet.SetBit(t5);
        bitSet.SetBit(t6);
        bitSet.SetBit(t7);
        bitSet.SetBit(t8);
        bitSet.SetBit(t9);
        bitSet.SetBit(t10);
    }
    #endregion

    // ----------------------------------------- component getter -----------------------------------------
#region component types query
    /// <summary>
    /// Return true if it contains the passed <see cref="IComponent"/> type <typeparamref name="T1"/>.
    /// </summary>
    public readonly bool    Has<T1> ()
        where T1 : struct, IComponent
    {
        return bitSet.Has(StructInfo<T1>.Index);
    }
    
    /// <summary>
    /// Return true if it contains all passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    public readonly bool    Has<T1, T2> ()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        return bitSet.Has(StructInfo<T1>.Index) &&
               bitSet.Has(StructInfo<T2>.Index);
    }

    /// <summary>
    /// Return true if it contains all passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/>.
    /// </summary>
    public readonly bool    Has<T1, T2, T3> ()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        return bitSet.Has(StructInfo<T1>.Index) &&
               bitSet.Has(StructInfo<T2>.Index) &&
               bitSet.Has(StructInfo<T3>.Index);
    }
    
    /// <summary>
    /// Return true if it contains all passed <paramref name="componentTypes"/>.
    /// </summary>
    public readonly bool HasAll (in ComponentTypes componentTypes)
    {
        return bitSet.HasAll(componentTypes.bitSet);
    }
    
    /// <summary>
    /// Return true if it contains any of the passed <paramref name="componentTypes"/>.
    /// </summary>
    public readonly bool HasAny (in ComponentTypes componentTypes)
    {
        return bitSet.HasAny(componentTypes.bitSet);
    }
    #endregion
    
#region component types mutation
    // ----------------------------------------- mutate Mask -----------------------------------------
    /// <summary>
    /// Add the passed <see cref="IComponent"/> type <typeparamref name="T"/>.
    /// </summary>
    public void Add<T>()
        where T : struct, IComponent
    {
        bitSet.SetBit(StructInfo<T>.Index);
    }
    
    /// <summary>
    /// Add all passed <paramref name="componentTypes"/>.
    /// </summary>
    public void Add(in ComponentTypes componentTypes)
    {
        bitSet.Add(componentTypes.bitSet);
    }
    
    /// <summary>
    /// Add the passed <see cref="IComponent"/> type <typeparamref name="T"/>.
    /// </summary>
    public void Remove<T>()
        where T : struct, IComponent
    {
        bitSet.ClearBit(StructInfo<T>.Index);
    }
    
    /// <summary>
    /// Remove all passed  <paramref name="componentTypes"/>.
    /// </summary>
    public void Remove(in ComponentTypes componentTypes)
    {
        bitSet.Remove(componentTypes.bitSet);
    }
    #endregion
    
    // ----------------------------------------- static methods -----------------------------------------
#region generic component types creation

    /// <summary>
    /// Create an instance containing the passed <see cref="IComponent"/> type <typeparamref name="T1"/>.
    /// </summary>
    public static ComponentTypes Get<T1>()
        where T1 : struct, IComponent
    {
        return Generic<T1>.ComponentTypes;
    }
    
    /// <summary>
    /// Create an instance containing the passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    public static ComponentTypes Get<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        return Generic<T1,T2>.ComponentTypes;
    }
    
    /// <summary>
    /// Create an instance containing the passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/>.
    /// </summary>
    public static ComponentTypes Get<T1, T2, T3>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        return Generic<T1,T2,T3>.ComponentTypes;
    }
    
    /// <summary>
    /// Create an instance containing the passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/> and <typeparamref name="T4"/>.
    /// </summary>
    public static ComponentTypes Get<T1, T2, T3, T4>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        return Generic<T1,T2,T3,T4>.ComponentTypes;
    }
    
    /// <summary>
    /// Create an instance containing the passed <see cref="IComponent"/> types <typeparamref name="T1"/>,
    /// <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>  and <typeparamref name="T4"/>.
    /// </summary>
    public static ComponentTypes Get<T1, T2, T3, T4, T5>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        return Generic<T1,T2,T3,T4,T5>.ComponentTypes;
    }
    /*
    internal static ComponentTypes Get<T1, T2, T3, T4, T5, T6>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        return new ComponentTypes(
            StructInfo<T1>.Index,
            StructInfo<T2>.Index,
            StructInfo<T3>.Index,
            StructInfo<T4>.Index,
            StructInfo<T5>.Index,
            StructInfo<T6>.Index);
    }
    
    internal static ComponentTypes Get<T1, T2, T3, T4, T5, T6, T7>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
    {
        return new ComponentTypes(
            StructInfo<T1>.Index,
            StructInfo<T2>.Index,
            StructInfo<T3>.Index,
            StructInfo<T4>.Index,
            StructInfo<T5>.Index,
            StructInfo<T6>.Index,
            StructInfo<T7>.Index);
    }
    
    internal static ComponentTypes Get<T1, T2, T3, T4, T5, T6, T7, T8>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
    {
        return new ComponentTypes(
            StructInfo<T1>.Index,
            StructInfo<T2>.Index,
            StructInfo<T3>.Index,
            StructInfo<T4>.Index,
            StructInfo<T5>.Index,
            StructInfo<T6>.Index,
            StructInfo<T7>.Index,
            StructInfo<T8>.Index);
    }
    
    internal static ComponentTypes Get<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
    {
        return new ComponentTypes(
            StructInfo<T1>.Index,
            StructInfo<T2>.Index,
            StructInfo<T3>.Index,
            StructInfo<T4>.Index,
            StructInfo<T5>.Index,
            StructInfo<T6>.Index,
            StructInfo<T7>.Index,
            StructInfo<T8>.Index,
            StructInfo<T9>.Index);
    }
    
    internal static ComponentTypes Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
        where T8 : struct, IComponent
        where T9 : struct, IComponent
        where T10: struct, IComponent
    {
        return new ComponentTypes(
            StructInfo<T1>.Index,
            StructInfo<T2>.Index,
            StructInfo<T3>.Index,
            StructInfo<T4>.Index,
            StructInfo<T5>.Index,
            StructInfo<T6>.Index,
            StructInfo<T7>.Index,
            StructInfo<T8>.Index,
            StructInfo<T9>.Index,
            StructInfo<T10>.Index);
    }*/
    #endregion
    
#region internal methods
    private string GetString() => AppendTo(new StringBuilder()).ToString();
    
    internal StringBuilder AppendTo(StringBuilder sb)
    {
        sb.Append("Components: [");
        var hasTypes    = false;
        var components  = EntityStoreBase.Static.EntitySchema.components;
        foreach (var index in bitSet) {
            var structType = components[index];
            sb.Append(structType.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
        }
        sb.Append(']');
        return sb;
    }
    #endregion
}

/// <summary>
/// Return the <see cref="IComponent"/> types of <see cref="ComponentTypes"/>.
/// </summary>
public struct ComponentTypesEnumerator : IEnumerator<ComponentType>
{
    internal                BitSetEnumerator    bitSetEnumerator;   // 48
    
    private static readonly ComponentType[]     Components = EntityStoreBase.Static.EntitySchema.components;

    // --- IEnumerator
    public          void            Reset()             => bitSetEnumerator.Reset();

           readonly object          IEnumerator.Current => Current;
           
    public readonly ComponentType   Current             => Components[bitSetEnumerator.Current];
    
    internal ComponentTypesEnumerator(in ComponentTypes componentTypes) {
        bitSetEnumerator = new BitSetEnumerator(componentTypes.bitSet);
    }
    
    // --- IEnumerator
    public          bool MoveNext() => bitSetEnumerator.MoveNext();
    public readonly void Dispose() { }
}

internal class ComponentTypesDebugView
{
    [Browse(RootHidden)]
    public              ComponentType[] Types => GetComponentTypes();

    [Browse(Never)]
    private readonly    ComponentTypes  componentTypes;
        
    internal ComponentTypesDebugView(ComponentTypes componentTypes) {
        this.componentTypes = componentTypes;
    }
    
    private ComponentType[] GetComponentTypes()
    {        
        var items = new ComponentType[componentTypes.Count];
        int n = 0;
        foreach (var type in componentTypes) {
            items[n++] = type;
        }
        return items;
    }
}