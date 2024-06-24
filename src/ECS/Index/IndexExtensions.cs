// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class IndexExtensions
{
    /// <summary>
    /// Return the entities having a component link to this entity of the passed <see cref="ILinkComponent"/> type.<br/>
    /// Executes in O(1). 
    /// </summary>
    public static Entities GetLinkingEntities<TComponent>(this Entity entity) where TComponent: struct, ILinkComponent {
        var index = (ComponentIndex<Entity>)StoreIndex.GetIndex(entity.store, StructInfo<TComponent>.Index);
        return index.GetHasValueEntities(entity);
    }
}