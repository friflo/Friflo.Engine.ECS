// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

namespace Friflo.Fliox.Engine.Client;

public class EntityStoreClient : FlioxClient
{
    public  readonly    EntitySet <int, DataNode>   entities;
    
    public EntityStoreClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}