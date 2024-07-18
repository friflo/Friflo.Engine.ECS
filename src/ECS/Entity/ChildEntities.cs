// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS.Collections;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Return the child entities of an <see cref="Entity"/>.
/// </summary>
[DebuggerTypeProxy(typeof(ChildEntitiesDebugView))]
public struct ChildEntities : IEnumerable<Entity>
{
    // --- public properties
                        public              int                 Count           => node.childIds.count;
                        public              ReadOnlySpan<int>   Ids             => node.ChildIds;
    
                        public              Entity              this[int index] => new Entity(store, Ids[index]);
                        public override     string              ToString()      => $"Entity[{Count}]";
    
    // --- internal fields
    /// must not be readonly. Because of <see cref="TreeNode.GetChildIds"/> using <see cref="MemoryMarshal.CreateReadOnlySpan{T}"/>
    [Browse(Never)]     internal            TreeNode            node;   // 16
    [Browse(Never)]     internal readonly   EntityStore         store;  //  8
    
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
    
    public void ToArray(Entity[] array) {
        var ids = Ids;
        for (int n = 0; n < ids.Length; n++) {
            array[n] = new Entity(store, ids[n]);
        }
    }
    
    internal Entity[] ToArray() {
        var ids = Ids;
        var array = new Entity[Ids.Length];
        for (int n = 0; n < Ids.Length; n++) {
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
    private             int         index;      //  4
    private readonly    TreeNode    node;       //  16
    private readonly    EntityStore store;      //  8
    
    internal ChildEnumerator(in ChildEntities childEntities) {
        node    = childEntities.node;
        store   = childEntities.store;
    }
    
    // --- IEnumerator<>
    public readonly Entity Current   => new Entity(store, node.childIds.Get(index - 1, node.arrayHeap));
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < node.childIds.count) {
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