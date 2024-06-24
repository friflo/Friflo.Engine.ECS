// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class IndexExtensions
{
    /// <summary>
    /// Return the entities referencing this entity using a <see cref="Entity"/> component index.<br/>
    /// Executes in O(1) with default index. 
    /// </summary>
    public static Entities GetForeignEntities<TComponent>(this Entity entity) where TComponent: struct, IIndexedComponent<Entity> {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(entity.store, StructInfo<TComponent>.Index);
        return index.GetHasValueEntities(entity);
    }
}