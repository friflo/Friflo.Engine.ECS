// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Client;

namespace Friflo.Fliox.Engine.Client.Attic;

internal interface IDatabaseSync
{
    bool                                        TryGetEntity (long pid, out DataEntity dataEntity);
    void                                        AddEntity    (DataEntity dataEntity);
    int                                         Count       { get; }
    IEnumerable<KeyValuePair<long, DataEntity>> Entities    { get; }
}

[Obsolete] [ExcludeFromCodeCoverage]
internal sealed class ClientSync : IDatabaseSync
{
    private readonly LocalEntities<long, DataEntity>  entities;
    
    public ClientSync(EntityClient client) {
        entities = client.entities.Local;
    }
        
    public bool     TryGetEntity(long pid, out DataEntity dataEntity)   => entities.TryGetEntity(pid, out dataEntity);
    public void     AddEntity(DataEntity dataEntity)                    => entities.Add(dataEntity);
    public int      Count                                               => entities.Count;
    public IEnumerable<KeyValuePair<long, DataEntity>> Entities         => entities;
}