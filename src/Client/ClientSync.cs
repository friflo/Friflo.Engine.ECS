// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox.Hub.Client;

namespace Friflo.Fliox.Engine.Client;

public class ClientSync : IDatabaseSync
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
}