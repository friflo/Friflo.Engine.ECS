// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

#if !UNITY_5_3_OR_NEWER

using System.Collections.Generic;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Engine.Hub;
using Friflo.Json.Fliox.Hub.Host;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.Hub {

public static class Test_StoreCommands
{
    private static StoreClient CreateClient(EntityStore store) {
        var commands    = new StoreCommands(store);
        var database    = new MemoryDatabase("test");
        database.AddCommands(commands);
        var hub         = new FlioxHub(database);
        return new StoreClient(hub);
    }
    
    [Test]
    public static async Task Test_StoreCommands_Collect()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var client  = CreateClient(store);
        {
            var collect = client.Collect(1);
            await client.SyncTasks();
            
            StringAssert.StartsWith("GC.Collect(1) - duration:", collect.Result);
        }
        // --- errors
        {
            var get     = client.SendCommand<string, string>("store.Collect", "foo");
            await client.TrySyncTasks();
            
            IsFalse(get.Success);
            var expect = "ValidationError ~ Incorrect type. was: 'foo', expect: int32 (root), pos: 5";
            AreEqual(expect, get.Error.Message);
        }
    }
    
    [Test]
    public static async Task Test_StoreCommands_GetEntities()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        store.CreateEntity(11);
        var client  = CreateClient(store);
        {
            var get     = client.GetEntities(new GetEntities { ids = new List<long> { 11 } });
            await client.SyncTasks();
            
            var getResult = get.Result;
            AreEqual(1, getResult.count);
        }
        // --- errors
        {
            var get     = client.GetEntities(new GetEntities { ids = new List<long> { 12 } });
            await client.TrySyncTasks();
            
            IsFalse(get.Success);
            AreEqual("CommandError ~ pid not found. was: 12", get.Error.Message);
        }
        {
            var get     = client.GetEntities(null);
            await client.TrySyncTasks();
            
            IsFalse(get.Success);
            AreEqual("CommandError ~ missing param", get.Error.Message);
        }
        {
            var get     = client.SendCommand<string, GetEntitiesResult>("store.GetEntities", "foo");
            await client.TrySyncTasks();
            
            IsFalse(get.Success);
            var expect = "ValidationError ~ Incorrect type. was: 'foo', expect: GetEntities (root), pos: 5";
            AreEqual(expect, get.Error.Message);
        }
    }
    
    [Test]
    public static async Task Test_StoreCommands_AddEntities()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        store.CreateEntity(11);
        var client  = CreateClient(store);
        {
            var dataEntity  = new DataEntity { pid = 2 };
            var entities    = new List<DataEntity> { dataEntity };
            var add         = client.AddEntities(new AddEntities { targetEntity = 11, entities = entities });
            await client.SyncTasks();
            
            var addResult = add.Result;
            AreEqual(1, addResult.count);
            AreEqual(1, addResult.newPids.Count);
            AreEqual(0, addResult.errors.Count);
        }
        // --- errors
        {
            var add         = client.AddEntities(new AddEntities { targetEntity = 99, entities = new List<DataEntity>() });
            await client.TrySyncTasks();
            
            IsFalse(add.Success);
            var expect = "CommandError ~ targetEntity not found. was: 99";
            AreEqual(expect, add.Error.Message);
        } {
            var add         = client.AddEntities(null);
            await client.TrySyncTasks();
            
            IsFalse(add.Success);
            var expect = "CommandError ~ missing param";
            AreEqual(expect, add.Error.Message);
        }
        {
            var add     = client.SendCommand<string, AddEntitiesResult>("store.AddEntities", "foo");
            await client.TrySyncTasks();
            
            IsFalse(add.Success);
            var expect = "ValidationError ~ Incorrect type. was: 'foo', expect: AddEntities (root), pos: 5";
            AreEqual(expect, add.Error.Message);
        }
    }
}

}

#endif