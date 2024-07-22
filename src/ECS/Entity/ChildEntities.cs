// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS.Collections;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Return the child entities of an <see cref="Entity"/>.<br/>
/// To iterate all entities with child entities use <see cref="TreeNode"/> in a <c>Query()</c>.
/// </summary>
[DebuggerTypeProxy(typeof(ChildEntitiesDebugView))]
public readonly struct ChildEntities : IEnumerable<Entity>
{
#region properties
    public          int                 Count           => node.childIds.count;
    public          ReadOnlySpan<int>   Ids             => node.GetChildIds(store);
    
    public          Entity              this[int index] => new Entity(store, node.childIds.GetAt(index, store.extension.childHeap));
    public override string              ToString()      => $"Entity[{Count}]";
    #endregion
    
#region fields
    [Browse(Never)]     internal readonly   TreeNode            node;   //  8
    [Browse(Never)]     internal readonly   EntityStore         store;  //  8
    #endregion
    
    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new ChildEnumerator(this);
    
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new ChildEnumerator(this);
    
    // --- new
    public ChildEnumerator                  GetEnumerator() => new ChildEnumerator(this);

    internal ChildEntities(Entity entity) {
        store = entity.store;
        entity.TryGetTreeNode(out node);
    }
    
    internal ChildEntities(EntityStore store, TreeNode node) {
        this.store  = store;
        this.node   = node;
    }
    
    public void ToArray(Entity[] array) {
        var ids = Ids;
        for (int n = 0; n < ids.Length; n++) {
            array[n] = new Entity(store, ids[n]);
        }
    }
    
    internal Entity[] ToArray() {
        var ids     = Ids;
        var array   = new Entity[ids.Length];
        for (int n = 0; n < ids.Length; n++) {
            array[n] = new Entity(store, ids[n]);
        }
        return array;
    }
}

/// <summary>
/// Use to enumerate the child entities stored in <see cref="Entity"/>.<see cref="Entity.ChildEntities"/>.  
/// </summary>
public struct ChildEnumerator : IEnumerator<Entity>
{
#region fields
    private             int         index;      //  4
    private readonly    IdArray     childIds;   //  8
    private readonly    EntityStore store;      //  8
    private readonly    IdArrayHeap heap;       //  8
    #endregion
    
    internal ChildEnumerator(in ChildEntities childEntities) {
        childIds    = childEntities.node.childIds;
        store       = childEntities.store;
        heap        = store.extension.childHeap;
    }
    
    // --- IEnumerator<>
    public readonly Entity Current   => new Entity(store, childIds.GetAt(index - 1, heap));
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < childIds.count) {
            index++;
            return true;
        }
        return false;
    }

    public void Reset() {
        index = 0;
    }
    
    object IEnumerator.Current => Current;

    // --- IDisposable
    public void Dispose() { }
}

internal class ChildEntitiesDebugView
{
    [Browse(RootHidden)]
    public  Entity[]        Entities => childEntities.ToArray();

    [Browse(Never)]
    private ChildEntities   childEntities;
        
    internal ChildEntitiesDebugView(ChildEntities childEntities)
    {
        this.childEntities = childEntities;
    }
}