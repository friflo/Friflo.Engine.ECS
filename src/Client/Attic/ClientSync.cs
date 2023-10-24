// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox.Hub.Client;

namespace Friflo.Fliox.Engine.Client.Attic;

internal interface IDatabaseSync
{
    bool                                            TryGetEntity (long pid, out DatabaseEntity databaseEntity);
    void                                            AddEntity    (DatabaseEntity databaseEntity);
    int                                             Count     { get; }
    IEnumerable<KeyValuePair<long, DatabaseEntity>> Entities        { get; }
}

[Obsolete] [ExcludeFromCodeCoverage]
internal sealed class ClientSync : IDatabaseSync
{
    private readonly LocalEntities<long, DatabaseEntity>  entities;
    
    public ClientSync(GameClient client) {
        entities = client.entities.Local;
    }
        
    public bool     TryGetEntity(long pid, out DatabaseEntity databaseEntity)   => entities.TryGetEntity(pid, out databaseEntity);
    public void     AddEntity(DatabaseEntity databaseEntity)                    => entities.Add(databaseEntity);
    public int      Count                                                       => entities.Count;
    public IEnumerable<KeyValuePair<long, DatabaseEntity>> Entities             => entities;
}