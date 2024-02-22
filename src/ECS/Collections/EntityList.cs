// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[DebuggerTypeProxy(typeof(EntityListDebugView))]
internal sealed class EntityList : IEnumerable<Entity>
{
#region properties
    public              int         Count       => count;
    public override     string      ToString()  => $"Count: {count}";
    #endregion
    
#region fields
    internal            int[]       ids;    //  8
    internal readonly   EntityStore store;  //  8
    internal            int         count;  //  4
    #endregion
    
    public EntityList(EntityStore store)
    {
        this.store = store;
        ids = new int[8];
    }
    
    public void Clear() {
        count = 0;
    }
    
    public void Add(Entity entity)
    {
        if (ids.Length == count) {
            ArrayUtils.Resize(ref ids, 2 * count);
        }
        ids[count++] = entity.Id;
    }
    
    public void Add(int entityId) {
        if (ids.Length == count) {
            ArrayUtils.Resize(ref ids, 2 * count);
        }
        ids[count++] = entityId;
    }
    
    public Entity this[int index] => new Entity(store, ids[index]);
    
    public EntityListEnumerator             GetEnumerator() => new EntityListEnumerator (this);

    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new EntityListEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new EntityListEnumerator (this);
}


internal struct EntityListEnumerator : IEnumerator<Entity>
{
    private readonly    int[]       ids;        //  8
    private readonly    EntityStore store;      //  8
    private readonly    int         count;      //  4
    private             int         index;      //  4
    private             Entity      current;    // 16
    
    internal EntityListEnumerator(EntityList list) {
        ids     = list.ids;
        count   = list.count;
        store   = list.store;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = 0;

    readonly object  IEnumerator.Current    => current;

    public   Entity              Current    => current;
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < count) {
            current = new Entity(store, ids[index++]);
            return true;
        }
        current = default;
        return false;
    }
    
    public readonly void Dispose() { }
}

internal sealed class EntityListDebugView
{
    [Browse(RootHidden)]
    public              Entity[]    Entities => GetEntities();
    
    private readonly    EntityList  list;
    
    internal EntityListDebugView(EntityList list) {
        this.list = list;
    }
    
    private Entity[] GetEntities()
    {
        var ids     = list.ids;
        var store   = list.store;
        var count   = list.count;
        var result  = new Entity[count];
        for (int n = 0; n < count; n++) {
            result[n] = new Entity(store, ids[n]);
        }
        return result;
    }
} 
