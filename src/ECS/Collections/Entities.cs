// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[DebuggerTypeProxy(typeof(EntitiesDebugView))]
public readonly struct Entities : IReadOnlyList<Entity>
{
#region properties
    public              int                 Count       => count;
    public              EntityStore         EntityStore => store;
    public              ReadOnlySpan<int>   Ids         => ids != null ? new ReadOnlySpan<int>(ids, start, count) : store.GetSpanId(start);
    public   override   string              ToString()  => $"Entity[{count}]";
    #endregion

#region interal fields
    internal readonly   int[]           ids;    //  8
    internal readonly   EntityStore     store;  //  8
    /// id - if <see cref="ids"/> == null.
    internal readonly   int             start;  //  4
    internal readonly   int             count;  //  4
    #endregion
    
#region general
    internal Entities(EntityStore store, int[] ids, int start, int count) {
        this.ids    = ids ?? throw new InvalidOperationException("expect ids != null");
        this.store  = store;
        this.start  = start;
        this.count  = count;
    }
    
    internal Entities(EntityStore store) {
        ids         = Array.Empty<int>();
        this.store  = store;
    }
    
    internal Entities(EntityStore store, int id) {
        this.store  = store;
        start       = id;
        count       = 1;
    }
    
    public Entity this[int index] {
        get {
            if (ids != null) {
                return new Entity(store, ids[start + index]);
            }
            // case: count == 1
            if (index != 0) throw new IndexOutOfRangeException();
            return new Entity(store, start);
        }
    }
    
    /// <summary>
    /// Return the entity ids as a string.<br/>E.g <c>"{ 1, 3, 7 }"</c>
    /// </summary>
    public string Debug()
    {
        if (count == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var entity in this) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(entity.Id);
        }
        sb.Append(" }");
        return sb.ToString();
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
        id      = entities.start;
        index   = start;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = start;

    readonly object  IEnumerator.Current    => new Entity(store, ids != null ? ids[index] : id);

    public   Entity              Current    => new Entity(store, ids != null ? ids[index] : id);
    
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