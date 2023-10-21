using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Database;
using Friflo.Json.Fliox.Hub.Host;

namespace Tests.ECS;

public static class TestUtils
{
    public static GameEntityStore CreateGameEntityStore(out GameDatabase database, PidType pidType = PidType.UsePidAsId) {
        var hub     = new FlioxHub(new MemoryDatabase("test"));
        var client  = new GameClient(hub);
        var sync    = new GameClientSync(client);
        var store   = new GameEntityStore(pidType);
        database    = new GameDatabase(store, sync);
        return store;
    }
}