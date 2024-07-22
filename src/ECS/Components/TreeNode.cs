// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// A <see cref="TreeNode"/> component stores the <see cref="Entity.ChildEntities"/> of an <see cref="Entity"/>.<br/>
/// It is used to build up an entity hierarchy used for scene graphs. 
/// </summary>
/// <remarks>
/// To change the <see cref="Entity.ChildEntities"/> of an <see cref="Entity"/> use:<br/>
/// - <see cref="Entity.AddChild"/><br/>
/// - <see cref="Entity.RemoveChild"/><br/>
/// - <see cref="Entity.InsertChild"/><br/>
/// <br/>
/// Internally the child entities of an entity are stored in up to a dozen int[] arrays.<br/>
/// If these array buffers grown large enough over time no heap allocations will happen if adding or removing child entities.<br/>
/// </remarks>
[ComponentKey(null)]
public struct TreeNode : IComponent
{
#region properties
    /// <summary> Returns the number of <see cref="Entity.ChildEntities"/>.</summary>
    public          int                 ChildCount  => childIds.count;
    
    /// <summary>Property is obsolete. Use <see cref="GetChildIds"/> instead. </summary>
    [Obsolete($"Use {nameof(GetChildIds)}()")]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public          ReadOnlySpan<int>   ChildIds    => throw new InvalidOperationException($"ChildIds is obsolete. Use {nameof(GetChildIds)}()");
    
    public override string              ToString()  => $"ChildCount: {childIds.count}";
    #endregion
    
#region fields
                    internal    IdArray childIds;   //  8
    #endregion
    
    /// <summary>
    /// Returns the child entities.<br/>
    /// Executes in O(1).
    /// </summary>
    public ChildEntities  GetChildEntities(EntityStore store) => new ChildEntities(store, this);
    
    // same as <see cref="IdArrayExtensions.GetSpan"/>
    /// <summary>
    /// Returns the child entity ids.<br/>
    /// Executes in O(1).
    /// </summary>
    public ReadOnlySpan<int>  GetChildIds(EntityStore store)
    {
        var count = childIds.count;
        var start = childIds.start;
        switch (count) {
            case 0: return default;
            case 1: return store.GetSpanId(start);
        }
        return new ReadOnlySpan<int>(IdArrayPool.GetIds(count, store.extension.childHeap), start, count);
    }
}