// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal ref struct EntitySpan
{
#region properties
    [Browse(RootHidden)]internal            Entities            Entities    => new Entities(Ids.ToArray(), Store, 0, Length);
    [Browse(Never)]     public              int                 Length      => id == 0 ? Ids.Length : 1;
    [Browse(Never)]     public              ReadOnlySpan<int>   Ids         => id == 0 ? ids        : MemoryMarshal.CreateReadOnlySpan(ref id, 1);
    
                        public   override   string              ToString()  => $"Entity[{Length}]";
    #endregion

#region fields
    [Browse(Never)]     public   readonly   EntityStore         Store;
    [Browse(Never)]     private             int                 id;
    [Browse(Never)]     private  readonly   ReadOnlySpan<int>   ids;
    #endregion
    
    internal EntitySpan(EntityStore store, ReadOnlySpan<int> ids, int id) {
        Store       = store;
        this.ids    = ids;
        this.id     = id;
    }
    
    public Entity this[int index] {
        get {
            if (id == 0) {
                return new Entity(Store, ids[index]);
            }
            return new Entity(Store, id);
        }
    }
    
    public EntitySpanEnumerator GetEnumerator() => new EntitySpanEnumerator(this);
}


internal ref struct EntitySpanEnumerator
{
    private  readonly   ReadOnlySpan<int>   ids;
    private  readonly   EntityStore         store;
    private             int                 index;
    
    internal EntitySpanEnumerator(EntitySpan span) {
        ids     = span.Ids;
        store   = span.Store;
        index   = -1;
    }
    
    public Entity Current => new Entity(store, ids[index]);
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < ids.Length - 1) {
            index++;
            return true;
        }
        return false;
    }
}
