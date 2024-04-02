// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct EntityStoreInfo
{
    /// <summary> Return the number of cached <see cref="EntityBatch"/>'s. </summary>
    public int PooledEntityBatchCount       => store.PooledEntityBatchCount;
    
    /// <summary> Return the number of cached <see cref="CreateEntityBatch"/>'s. </summary>
    public int PooledCreateEntityBatchCount => store.PooledCreateEntityBatchCount;
        
    private readonly EntityStore store;
    
    internal EntityStoreInfo(EntityStore store) {
        this.store = store;
    }
}