// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public readonly struct ChildEntities // : IEnumerable <Entity>  // <- not implemented to avoid boxing
{
    // --- public properties
    [Browse(Never)]     public              int                 Length          => childLength;
    [Browse(Never)]     public              ReadOnlySpan<int>   Ids             => new (childIds, 0, childLength);
    
/*  /// <summary>Property <b>only used</b> to display child entities in Debugger. See <see cref="ChildEntities"/> remarks.</summary>
    [Obsolete($"use either {nameof(ChildEntities)}[], {nameof(ChildEntities)}.{nameof(ToArray)}() or foreach (var node in entity.{nameof(ChildEntities)})")]
    [Browse(RootHidden)]public              Entity[]            Entities_       => GetEntities(); */
                        public              Entity              this[int index] => new Entity(Ids[index], store);
                        public override     string              ToString()      => $"Length: {childLength}";
    
    // --- internal fields
    [Browse(Never)]     internal readonly   int                 childLength;    //  4
    [Browse(Never)]     internal readonly   int[]               childIds;       //  8
    [Browse(Never)]     internal readonly   EntityStore         store;          //  8


    public ChildEnumerator GetEnumerator() => new ChildEnumerator(this);

    internal ChildEntities(EntityStore store, int[] childIds, int childLength) {
        this.store          = store;
        this.childIds       = childIds;
        this.childLength    = childLength;
    }
    
    public void ToArray(Entity[] array) {
        var ids = Ids;
        for (int n = 0; n < childLength; n++) {
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
    
    /* // intentionally not implemented to avoid boxing. See comment above     
    public IEnumerator<Entity> GetEnumerator()      => throw new System.InvalidOperationException();
    IEnumerator IEnumerable.GetEnumerator()         => throw new System.InvalidOperationException();
    */
}

public struct ChildEnumerator // : IEnumerator<EntityNode> // <- not implemented to enable returning Current by ref
{
    private             int             index;          //  4
    private readonly    ChildEntities   childEntities;  // 20
    
    internal ChildEnumerator(in ChildEntities childEntities) {
        this.childEntities = childEntities;
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Entity Current   => new Entity(childEntities.childIds[index - 1], childEntities.store);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < childEntities.childLength) {
            index++;
            return true;
        }
        return false;  
    }

    public void Reset() {
        index = 0;
    }
    // object IEnumerator.Current => Current;                                           // not implemented: see comment above

    // --- IEnumerator<>
    // public EntityNode Current   => childNodes.nodes[childNodes.childIds[index - 1]]; // not implemented: see comment above

    public void Dispose() { }
}


