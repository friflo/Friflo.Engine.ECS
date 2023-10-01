using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

namespace Fliox.Engine.Client;

public class EntityStoreClient : FlioxClient
{
    public  readonly    EntitySet <int, DataNode>   entities;
    
    public EntityStoreClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}