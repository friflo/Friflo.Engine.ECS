// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[DebuggerTypeProxy(typeof(EntitiesDebugView))]
public struct Entities : IReadOnlyList<Entity>
{
#region properties
    public              int                 Count       => count;
    public              EntityStore         EntityStore => store;
    public              ReadOnlySpan<int>   Ids         => id == 0 ? new ReadOnlySpan<int>(ids, start, count) : MemoryMarshal.CreateReadOnlySpan(ref id, 1);
    
    public   override   string              ToString()  => $"Entity[{count}]";
    #endregion

#region interal fields
    internal readonly   int[]           ids;    //  8
    internal readonly   EntityStore     store;  //  8
    internal readonly   int             start;  //  4
    internal readonly   int             count;  //  4
    internal            int             id;     //  4
    #endregion
    
#region general
    internal Entities(EntityStore store, int[] ids, int start, int count) {
        this.ids    = ids;
        this.store  = store;
        this.start  = start;
        this.count  = count;
    }
    
    internal Entities(EntityStore store, int id) {
        this.store  = store;
        this.id     = id;
        count       = id == 0 ? 0 : 1;
    }
    
    public Entity this[int index] {
        get {
            if (id == 0) {
                return new Entity(store, ids[start + index]);
            }
            return new Entity(store, id);
        }
    }
    #endregion

    
#region IEnumerator
    public EntityEnumerator                 GetEnumerator() => new EntityEnumerator (this);
    
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new EntityEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new EntityEnumerator (this);
    #endregion
}


public struct EntityEnumerator : IEnumerator<Entity>
{
    private readonly    int[]       ids;        //  8
    private readonly    EntityStore store;      //  8
    private readonly    int         start;      //  4
    private readonly    int         last;       //  4
    private readonly    int         id;         //  4
    private             int         index;      //  4
    
    internal EntityEnumerator(in Entities entities) {
        ids     = entities.ids;
        store   = entities.store;
        start   = entities.start - 1;
        last    = start + entities.count;
        id      = entities.id;
        index   = start;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = start;

    readonly object  IEnumerator.Current    => new Entity(store, id == 0 ? ids[index] : id);

    public   Entity              Current    => new Entity(store, id == 0 ? ids[index] : id);
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < last) {
            index++;
            return true;
        }
        return false;
    }
    
    public readonly void Dispose() { }
}

internal sealed class EntitiesDebugView
{
    [Browse(RootHidden)]
    internal            Entity[]    Entities => GetEntities();
    
    private readonly    Entities    entities;
    
    internal EntitiesDebugView(Entities entities) {
        this.entities = entities;
    }
    
    private Entity[] GetEntities()
    {
        var result  = new Entity[entities.Count];
        int index   = 0;
        foreach (var entity in entities) {
            result[index++] = entity;
        }
        return result;
    }
} 