// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal sealed class EntityIndexValues : IReadOnlyCollection<Entity>
{
    public              int         Count => entityIndex.Count;

    private readonly    EntityIndex entityIndex;
    
    internal EntityIndexValues (EntityIndex entityIndex) {
        this.entityIndex = entityIndex;
    }

    public IEnumerator<Entity> GetEnumerator() => new EntityIndexValuesEnumerator(entityIndex);
    IEnumerator    IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

internal sealed class EntityIndexValuesEnumerator : IEnumerator<Entity>
{
    private             Dictionary<int,IdArray>.KeyCollection.Enumerator    enumerator;
    private readonly    EntityStore                                         store;
    
    internal EntityIndexValuesEnumerator(EntityIndex entityIndex) {
        enumerator  = entityIndex.entityMap.Keys.GetEnumerator();
        store       = entityIndex.store;
    }

    // --- IDisposable
    public  void    Dispose()           => enumerator.Dispose();

    // --- IEnumerator
    public  bool    MoveNext()          => enumerator.MoveNext();
    public  void    Reset()             => throw new NotImplementedException();
            object  IEnumerator.Current => throw new NotImplementedException();

    // --- IEnumerator<>
    public  Entity  Current             => new Entity(store, enumerator.Current);
}
