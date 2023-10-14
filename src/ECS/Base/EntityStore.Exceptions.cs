// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    internal static Exception InvalidStoreException(string parameterName) {
        return new ArgumentException("entity is owned by a different store", parameterName);
    }
        
    private static Exception InvalidEntityIdException(int id, string parameterName) {
        return new ArgumentException($"invalid node id <= 0. was: {id}", parameterName);
    }
        
    private static Exception IdAlreadyInUseException(int id, string parameterName) {
        return new ArgumentException($"id already in use in EntityStore. was: {id}", parameterName);
    }
        
    private static Exception EntityAlreadyHasParent(int child, int curParent, int newParent) {
        var msg = $"child has already a parent. child: {child} current parent: {curParent}, new parent: {newParent}";
        return new InvalidOperationException(msg);
    }
}