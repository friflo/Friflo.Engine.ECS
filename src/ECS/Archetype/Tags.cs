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

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// <see cref="Tags"/> define a set of <see cref="ITag"/>'s used to query entities in an <see cref="EntityStoreBase"/>.
/// </summary>
[CLSCompliant(true)]
[DebuggerTypeProxy(typeof(TagsDebugView))]
public struct Tags : IEnumerable<TagType>, IEquatable<Tags>
{
    internal            BitSet   bitSet;  // 32
    
    public readonly     TagsEnumerator  GetEnumerator()                         => new TagsEnumerator (this);

    // --- IEnumerable
           readonly     IEnumerator     IEnumerable.GetEnumerator()             => new TagsEnumerator (this);

    // --- IEnumerable<>
    readonly IEnumerator<TagType>       IEnumerable<TagType>.GetEnumerator()    => new TagsEnumerator (this);

    public  readonly override string    ToString() => GetString();
    
    // ----------------------------------- specialized constructor  -----------------------------------
    public Tags(TagType type)
    {
        bitSet.SetBit(type.TagIndex);
    }
    
    public Tags(in BitSet value)
    {
        bitSet = value;
    }
    
    // ----------------------------------------- read Tags -----------------------------------------
    /// <summary>Return the number of contained <see cref="ITag"/>'s.</summary>
    public readonly int     Count => bitSet.GetBitCount();
    
    /// <summary>
    /// Return true if it contain the passed tag <see cref="IComponent"/> type <typeparamref name="T1"/>.
    /// </summary>
    public readonly bool    Has<T1> ()
        where T1 : struct, ITag
    {
        return bitSet.Has(TagInfo<T1>.Index);
    }

    /// <summary>
    /// Return true if it contains all passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    public readonly bool    Has<T1, T2> ()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        return bitSet.Has(TagInfo<T1>.Index) &&
               bitSet.Has(TagInfo<T2>.Index);
    }

    /// <summary>
    /// Return true if it contains all passed <see cref="IComponent"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/>.
    /// </summary>
    public readonly bool    Has<T1, T2, T3> ()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
    {
        return bitSet.Has(TagInfo<T1>.Index) &&
               bitSet.Has(TagInfo<T2>.Index) &&
               bitSet.Has(TagInfo<T3>.Index);
    }
    
    /// <summary>
    /// Return true if it contains all passed <paramref name="tags"/>.
    /// </summary>
    public readonly bool HasAll (in Tags tags)
    {
        return bitSet.HasAll(tags.bitSet);
    }
    
    /// <summary>
    /// Return true if it contains any of the passed <paramref name="tags"/>.
    /// </summary>
    public readonly bool HasAny (in Tags tags)
    {
        return bitSet.HasAny(tags.bitSet);
    }
    
    // --- equality
    
    public override int     GetHashCode()           => throw new NotImplementedException("to prevent boxing");
    public override bool    Equals(object other)    => throw new NotImplementedException("to prevent boxing");
    public bool             Equals(Tags other)      => bitSet.Equals(other.bitSet);
    
    public static   bool    operator == (in Tags a, in Tags b)  => a.bitSet.Equals(b.bitSet);
    
    public static   bool    operator != (in Tags a, in Tags b)  => !a.bitSet.Equals(b.bitSet);
    
    // ----------------------------------------- mutate Tags -----------------------------------------
    /// <summary> Add the passed <see cref="ITag"/> type <typeparamref name="T"/>.</summary>
    public void Add<T>()
        where T : struct, ITag
    {
        bitSet.SetBit(TagInfo<T>.Index);
    }
    
    /// <summary> Add the passed <paramref name="tags"/>.</summary>
    public void Add(in Tags tags)
    {
        bitSet.Add(tags.bitSet);
    }
    
    /// <summary> Removes the passed <see cref="ITag"/> type <typeparamref name="T"/>.</summary>
    public void Remove<T>()
        where T : struct, ITag
    {
        bitSet.ClearBit(TagInfo<T>.Index);
    }
    
    /// <summary> Removes the passed <paramref name="tags"/>.</summary>
    public void Remove(in Tags tags)
    {
        bitSet.Remove(tags.bitSet);
    }
        
    // ----------------------------------------- static methods -----------------------------------------
    /// <summary>
    /// Create an instance containing the given <see cref="ITag"/> type <typeparamref name="T"/>.
    /// </summary>
    public static Tags Get<T>()
        where T : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagInfo<T>.Index);
        return tags;
    }
    
    /// <summary>
    /// Create an instance containing the given <see cref="ITag"/> types
    /// <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    public static Tags Get<T1, T2>()
        where T1 : struct, ITag
        where T2 : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagInfo<T1>.Index);
        tags.bitSet.SetBit(TagInfo<T2>.Index);
        return tags;
    }
    
    /// <summary>
    /// Create an instance containing the given <see cref="ITag"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/>.
    /// </summary>
    public static Tags Get<T1, T2, T3>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagInfo<T1>.Index);
        tags.bitSet.SetBit(TagInfo<T2>.Index);
        tags.bitSet.SetBit(TagInfo<T3>.Index);
        return tags;
    }
    
    /// <summary>
    /// Create an instance containing the given <see cref="ITag"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/> and <typeparamref name="T4"/>.
    /// </summary>
    public static Tags Get<T1, T2, T3, T4>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        where T4 : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagInfo<T1>.Index);
        tags.bitSet.SetBit(TagInfo<T2>.Index);
        tags.bitSet.SetBit(TagInfo<T3>.Index);
        tags.bitSet.SetBit(TagInfo<T4>.Index);
        return tags;
    }
    
    /// <summary>
    /// Create an instance containing the given <see cref="ITag"/> types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/> and
    /// <typeparamref name="T5"/>.
    /// </summary>
    public static Tags Get<T1, T2, T3, T4, T5>()
        where T1 : struct, ITag
        where T2 : struct, ITag
        where T3 : struct, ITag
        where T4 : struct, ITag
        where T5 : struct, ITag
    {
        var tags = new Tags();
        tags.bitSet.SetBit(TagInfo<T1>.Index);
        tags.bitSet.SetBit(TagInfo<T2>.Index);
        tags.bitSet.SetBit(TagInfo<T3>.Index);
        tags.bitSet.SetBit(TagInfo<T4>.Index);
        tags.bitSet.SetBit(TagInfo<T5>.Index);
        return tags;
    }
    
    private readonly string GetString()
    {
        var sb          = new StringBuilder();
        var hasTypes    = false;
        sb.Append("Tags: [");
        var tagTypes    = EntityStoreBase.Static.EntitySchema.tags;
        foreach (var index in bitSet) {
            var tagType = tagTypes[index];
            sb.Append('#');
            sb.Append(tagType.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
        }
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
/// Used to enumerate the <see cref="ITag"/>'s stored in <see cref="Tags"/>.
/// </summary>
public struct TagsEnumerator : IEnumerator<TagType>
{
    private                 BitSetEnumerator    bitSetEnumerator;   // 48
    private static readonly TagType[]           TagTypes = EntityStoreBase.Static.EntitySchema.tags;

    // --- IEnumerator
    public          void    Reset()             => bitSetEnumerator.Reset();

           readonly object  IEnumerator.Current => Current;

    public readonly TagType Current             => TagTypes[bitSetEnumerator.Current];
    
    internal TagsEnumerator(in Tags tags) {
        bitSetEnumerator = new BitSetEnumerator(tags.bitSet);
    }
    
    // --- IEnumerator
    public          bool MoveNext() => bitSetEnumerator.MoveNext();
    public readonly void Dispose() { }
}

internal class TagsDebugView
{
    [Browse(RootHidden)]
    public              TagType[]   TagTypes => GetItems();

    [Browse(Never)]
    private readonly    Tags        tags;
        
    internal TagsDebugView(Tags tags) {
        this.tags = tags;
    }
    
    private TagType[] GetItems() {
        
        var items = new TagType[tags.Count];
        int n = 0;
        foreach (var tag in tags) {
            items[n++] = tag;
        }
        return items;
    }
}