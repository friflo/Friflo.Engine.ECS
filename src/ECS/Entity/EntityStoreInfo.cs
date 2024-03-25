// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct EntityStoreInfo
{
    public int PooledEntityBatchCount       => store.PooledEntityBatchCount;
    public int PooledCreateEntityBatchCount => store.PooledCreateEntityBatchCount;
        
    private readonly EntityStore store;
    
    internal EntityStoreInfo(EntityStore store) {
        this.store = store;
    }
}