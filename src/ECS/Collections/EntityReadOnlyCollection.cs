// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public class EntityReadOnlyCollection : IReadOnlyCollection<Entity>
{
#region properties
    public              int                 Count   => collection.Count;
    public              EntityStore         Store   => store;
    
    public   override   string              ToString()  => $"Entity[{Count}]";
    #endregion

#region interal fields
    internal readonly   IReadOnlyCollection<int>    collection; //  8
    private  readonly   EntityStore                 store;      //  8
    #endregion
    
#region general
    internal EntityReadOnlyCollection(EntityStore store, IReadOnlyCollection<int> collection) {
        this.collection = collection;
        this.store      = store;
    }
    #endregion

    
#region IEnumerator
    public EntityReadOnlyCollectionEnumerator GetEnumerator() => new EntityReadOnlyCollectionEnumerator (this);
    
    // --- IEnumerable
    IEnumerator                   IEnumerable.GetEnumerator() => new EntityReadOnlyCollectionEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<Entity>   IEnumerable<Entity>.GetEnumerator() => new EntityReadOnlyCollectionEnumerator (this);
    #endregion
}


public readonly struct EntityReadOnlyCollectionEnumerator : IEnumerator<Entity>
{
    private readonly    IEnumerator<int>    enumerator;
    private readonly    EntityStore         store;
    
    internal EntityReadOnlyCollectionEnumerator(EntityReadOnlyCollection entities) {
        store       = entities.Store;
        enumerator  = entities.collection.GetEnumerator();
    }
    
    // --- IEnumerator<>
    public   Entity Current     => new Entity(store, enumerator.Current);
    
    // --- IEnumerator
    object  IEnumerator.Current => new Entity(store, enumerator.Current);
    public  bool    MoveNext()  => enumerator.MoveNext();
    public  void    Reset()     => enumerator.Reset();
    
    public  void    Dispose()   => enumerator.Dispose();
}
