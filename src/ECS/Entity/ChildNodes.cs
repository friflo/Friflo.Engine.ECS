// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct ChildEntities : IEnumerable<Entity>
{
    // --- public properties
    [Browse(Never)]     public              int                 Count           => childCount;
    [Browse(Never)]     public              ReadOnlySpan<int>   Ids             => new (childIds, 0, childCount);
    
/*  /// <summary>Property <b>only used</b> to display child entities in Debugger. See <see cref="ChildEntities"/> remarks.</summary>
    [Obsolete($"use either {nameof(ChildEntities)}[], {nameof(ChildEntities)}.{nameof(ToArray)}() or foreach (var node in entity.{nameof(ChildEntities)})")]
    [Browse(RootHidden)]public              Entity[]            Entities_       => GetEntities(); */
                        public              Entity              this[int index] => new Entity(Ids[index], store);
                        public override     string              ToString()      => $"Count: {childCount}";
    
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

    internal ChildEntities(EntityStore store, int[] childIds, int childCount) {
        this.store          = store;
        this.childIds       = childIds;
        this.childCount     = childCount;
    }
    
    public void ToArray(Entity[] array) {
        var ids = Ids;
        for (int n = 0; n < childCount; n++) {
            array[n] = new Entity(ids[n], store);
        }
    }

    /* was used for class Entity
    private Entity[] GetEntities() {
        var childEntities = new Entity[childLength];
        for (int n = 0; n < childLength; n++) {
            childEntities[n] = new Entity(childIds[n], store);
        }
        return childEntities;
    } */
}

public struct ChildEnumerator  : IEnumerator<Entity>
{
    private             int             index;          //  4
    private readonly    ChildEntities   childEntities;  // 20
    
    internal ChildEnumerator(in ChildEntities childEntities) {
        this.childEntities = childEntities;
    }
    
    // --- IEnumerator<>
    public readonly Entity Current   => new Entity(childEntities.childIds[index - 1], childEntities.store);
    
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


