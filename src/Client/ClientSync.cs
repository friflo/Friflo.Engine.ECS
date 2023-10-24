// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox.Hub.Client;

namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public sealed class ClientSync : IDatabaseSync
{
    private readonly LocalEntities<long, DatabaseEntity>  entities;
    
    public ClientSync(GameClient client) {
        entities = client.entities.Local;
    }
        
    public bool TryGetEntity(long pid, out DatabaseEntity databaseEntity) {
        return entities.TryGetEntity(pid, out databaseEntity);
    }

    public void AddEntity(DatabaseEntity databaseEntity) {
        entities.Add(databaseEntity);
    }

    public int EntityCount  => entities.Count;


    public IEnumerable<KeyValuePair<long, DatabaseEntity>> Entities => entities;
}