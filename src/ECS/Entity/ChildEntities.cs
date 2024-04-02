// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
public readonly struct ChildEntities : IEnumerable<Entity>
{
    // --- public properties
                        public              int                 Count           => childCount;
                        public              ReadOnlySpan<int>   Ids             => new (childIds, 0, childCount);
    
                        public              Entity              this[int index] => new Entity(store, Ids[index]);
                        public override     string              ToString()      => $"Entity[{childCount}]";
    
    // --- internal fields
    [Browse(Never)]     internal readonly   int                 childCount;     //  4
    [Browse(Never)]     internal readonly   int[]               childIds;       //  8
    [Browse(Never)]     internal readonly   EntityStore         store;          //  8

    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new ChildEnumerator(this);
    
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new ChildEnumerator(this);
    
    // --- new
    public ChildEnumerator                  GetEnumerator() => new ChildEnumerator(this);

    internal ChildEntities(EntityStore store, in EntityNode node) {
        this.store          = store;
        this.childIds       = node.childIds;
        this.childCount     = node.childCount;
    }
    
    public void ToArray(Entity[] array) {
        var ids = Ids;
        for (int n = 0; n < childCount; n++) {
            array[n] = new Entity(store, ids[n]);
        }
    }
    
    internal Entity[] ToArray() {
        var ids = Ids;
        var array = new Entity[childCount];
        for (int n = 0; n < childCount; n++) {
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
    private             int             index;          //  4
    private readonly    ChildEntities   childEntities;  // 20
    
    internal ChildEnumerator(in ChildEntities childEntities) {
        this.childEntities = childEntities;
    }
    
    // --- IEnumerator<>
    public readonly Entity Current   => new Entity(childEntities.store, childEntities.childIds[index - 1]);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < childEntities.childCount) {
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