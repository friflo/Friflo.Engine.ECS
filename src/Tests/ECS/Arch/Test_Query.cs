using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static Tests.Utils.Mem;

// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_Query
{
    [Test]
    public static void Test_SignatureTypes()
    {
        var sig1            = Signature.Get<Position>();
        AreEqual("Signature: [Position]",   sig1.ToString());
        AreEqual("Components: [Position]",  sig1.ComponentTypes.ToString());
        
        var sig2            = Signature.Get<Position, Rotation>();
        AreEqual("Signature: [Position, Rotation]", sig2.ToString());

        int count = 0;
        foreach (var _ in sig2.ComponentTypes) {
            count++;    
        }
        AreEqual(2, count);
    }
    
    [Test]
    public static void Test_Signature_Get_Mem()
    {
        Signature.Get<Position>();  // force one time allocation
        
        var start   = GetAllocatedBytes();
        
        var sig1 = Signature.Get<Position>();
        var sig2 = Signature.Get<Position, Rotation>();
        var sig3 = Signature.Get<Position, Rotation, Scale3>();
        var sig4 = Signature.Get<Position, Rotation, Scale3, MyComponent1>();
        var sig5 = Signature.Get<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        
        AssertNoAlloc(start);
        
        AreEqual("Components: [Position]",                                                sig1.ComponentTypes.ToString());
        AreEqual("Components: [Position, Rotation]",                                      sig2.ComponentTypes.ToString());
        AreEqual("Components: [Position, Rotation, Scale3]",                              sig3.ComponentTypes.ToString());
        AreEqual("Components: [Position, Rotation, Scale3, MyComponent1]",                sig4.ComponentTypes.ToString());
        AreEqual("Components: [Position, Rotation, Scale3, MyComponent1, MyComponent2]",  sig5.ComponentTypes.ToString());
    }
    
    [Test]
    public static void Test_generic_Query_with_AllTags()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity();
        entity1.AddComponent<Position>();
        entity1.AddComponent<Rotation>();
        entity1.AddComponent<Scale3>();
        entity1.AddComponent<MyComponent1>();
        entity1.AddComponent<MyComponent2>();

        var entity2 = store.CreateEntity();
        entity2.AddComponent<Position>();
        entity2.AddComponent<Rotation>();
        entity2.AddComponent<Scale3>();
        entity2.AddComponent<MyComponent1>();
        entity2.AddComponent<MyComponent2>();
        
        var query1 =    store.Query<Position>()                                                 .WithDisabled();
        var query2 =    store.Query<Position, Rotation>()                                       .WithDisabled();
        var query3 =    store.Query<Position, Rotation, Scale3>()                               .WithDisabled();
        var query4 =    store.Query<Position, Rotation, Scale3, MyComponent1>()                 .WithDisabled();
        var query5 =    store.Query<Position, Rotation, Scale3, MyComponent1, MyComponent2>()   .WithDisabled();
        
        AreEqual("Query: [Position]  Count: 2",                                               query1.ToString());
        AreEqual("Query: [Position, Rotation]  Count: 2",                                     query2.ToString());
        AreEqual("Query: [Position, Rotation, Scale3]  Count: 2",                             query3.ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1]  Count: 2",               query4.ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1, MyComponent2]  Count: 2", query5.ToString());
        
        AreEqual("QueryChunks[1]  Components: [Position]",                                                  query1.Chunks.ToString());
        AreEqual("QueryChunks[1]  Components: [Position, Rotation]",                                        query2.Chunks.ToString());
        AreEqual("QueryChunks[1]  Components: [Position, Rotation, Scale3]",                                query3.Chunks.ToString());
        AreEqual("QueryChunks[1]  Components: [Position, Rotation, Scale3, MyComponent1]",                  query4.Chunks.ToString());
        AreEqual("QueryChunks[1]  Components: [Position, Rotation, Scale3, MyComponent1, MyComponent2]",    query5.Chunks.ToString());
        
        AreEqual(2, query1.Chunks.Count);
        AreEqual(2, query2.Chunks.Count);
        AreEqual(2, query3.Chunks.Count);
        AreEqual(2, query4.Chunks.Count);
        AreEqual(2, query5.Chunks.Count);
        
        var tags = Tags.Get<TestTag>();
        AreEqual("Query: [Position, #TestTag]  Count: 0",                                                 query1.AllTags(tags).ToString());
        AreEqual("Query: [Position, Rotation, #TestTag]  Count: 0",                                       query2.AllTags(tags).ToString());
        AreEqual("Query: [Position, Rotation, Scale3, #TestTag]  Count: 0",                               query3.AllTags(tags).ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1, #TestTag]  Count: 0",                 query4.AllTags(tags).ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1, MyComponent2, #TestTag]  Count: 0",   query5.AllTags(tags).ToString());
    }
    
    [Test]
    public static void Test_generic_Query_QueryFilter()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity();
        entity1.AddComponent<Position>();
        entity1.AddComponent<Rotation>();
        entity1.AddComponent<Scale3>();
        entity1.AddComponent<MyComponent1>();
        entity1.AddComponent<MyComponent2>();
        entity1.AddTag<TestTag>();
        entity1.AddTag<TestTag2>();
        
        var filter = new QueryFilter()
            .AllTags        (Tags.Get<TestTag>())
            .AnyTags        (Tags.Get<TestTag2>())
            .WithoutAllTags (Tags.Get<TestTag3>())
            .WithoutAnyTags (Tags.Get<TestTag4>())
            .AllComponents          (ComponentTypes.Get<Position, Rotation>())
            .AnyComponents          (ComponentTypes.Get<Scale3>())
            .WithoutAllComponents   (ComponentTypes.Get<MyComponent1, MyComponent2, MyComponent3>())
            .WithoutAnyComponents   (ComponentTypes.Get<MyComponent4>());
        
        var query0 =    store.Query(filter).FreezeFilter();
        var query1 =    store.Query<Position>(filter).FreezeFilter();
        var query2 =    store.Query<Position, Rotation>(filter).FreezeFilter();
        var query3 =    store.Query<Position, Rotation, Scale3>(filter).FreezeFilter();
        var query4 =    store.Query<Position, Rotation, Scale3, MyComponent1>(filter).FreezeFilter();
        var query5 =    store.Query<Position, Rotation, Scale3, MyComponent1, MyComponent2>(filter).FreezeFilter();
        
        AreEqual(1, query0.Entities.Count);
        AreEqual(1, query1.Entities.Count);
        AreEqual(1, query2.Entities.Count);
        AreEqual(1, query3.Entities.Count);
        AreEqual(1, query4.Entities.Count);
        AreEqual(1, query5.Entities.Count);
    }
    
    [Test]
    public static void Test_generic_Query_QueryFilter_exceptions()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var filter = new QueryFilter().FreezeFilter();

        var e = Assert.Throws<InvalidOperationException>(() => {
            store.Query(filter).AllTags(Tags.Get<TestTag>()); 
        });
        Assert.AreEqual("QueryFilter was frozen and cannot be changed anymore.", e!.Message);
    }
    
    [Test]
    public static void Test_Query_ForEachEntity()
    {
        var store   = new EntityStore(PidType.RandomPids);
        for (int n = 1; n <= 5; n++) {
            store.Batch()
                .Add(new Position   (n, 0, 0))
                .Add(new Rotation   (n, 0, 0, 0))
                .Add(new Scale3     (n, 0, 0))
                .Add(new MyComponent1 { a = n })
                .Add(new MyComponent2 { b = n })
                .CreateEntity();
        }
        var query1 = store.Query<Position>();
        var query2 = store.Query<Position, Rotation>();
        var query3 = store.Query<Position, Rotation, Scale3>();
        var query4 = store.Query<Position, Rotation, Scale3, MyComponent1>();
        var query5 = store.Query<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        {
            int count = 0;
            query1.ForEachEntity((ref Position position, Entity entity) => {
                count++;
                AreEqual(count, position.x);
                AreEqual(count, entity.Id);
            });
            AreEqual(5, count);
        }
        {
            int count = 0;
            query2.ForEachEntity((ref Position pos, ref Rotation rot, Entity entity) => {
                count++;
                AreEqual(count, pos.x);
                AreEqual(count, rot.x);
                AreEqual(count, entity.Id);
            });
            AreEqual(5, count);
        }
        {
            int count = 0;
            query3.ForEachEntity((ref Position pos, ref Rotation rot, ref Scale3 scale, Entity entity) => {
                count++;
                AreEqual(count, pos.x);
                AreEqual(count, rot.x);
                AreEqual(count, scale.x);
                AreEqual(count, entity.Id);
            });
            AreEqual(5, count);
        }
        {
            int count = 0;
            query4.ForEachEntity((ref Position pos, ref Rotation rot, ref Scale3 scale, ref MyComponent1 my1, Entity entity) => {
                count++;
                AreEqual(count, pos.x);
                AreEqual(count, rot.x);
                AreEqual(count, scale.x);
                AreEqual(count, my1.a);
                AreEqual(count, entity.Id);
            });
            AreEqual(5, count);
        }
        {
            int count = 0;
            query5.ForEachEntity((ref Position pos, ref Rotation rot, ref Scale3 scale, ref MyComponent1 my1, ref MyComponent2 my2, Entity entity) => {
                count++;
                AreEqual(count, pos.x);
                AreEqual(count, rot.x);
                AreEqual(count, scale.x);
                AreEqual(count, my1.a);
                AreEqual(count, my2.b);
                AreEqual(count, entity.Id);
            });
            AreEqual(5, count);
        }
    }
    
    [Test]
    public static void Test_Signature_Query()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        
        var sig1 = Signature.Get<Position>();
        var sig2 = Signature.Get<Position, Rotation>();
        var sig3 = Signature.Get<Position, Rotation, Scale3>();
        var sig4 = Signature.Get<Position, Rotation, Scale3, MyComponent1>();
        var sig5 = Signature.Get<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        //
        var query1 =    store.Query(sig1);
        var query2 =    store.Query(sig2);
        var query3 =    store.Query(sig3);
        var query4 =    store.Query(sig4);
        var query5 =    store.Query(sig5);
        
        AreEqual("Query: [Position]  Count: 0",                                               query1.ToString());
        AreEqual("Query: [Position, Rotation]  Count: 0",                                     query2.ToString());
        AreEqual("Query: [Position, Rotation, Scale3]  Count: 0",                             query3.ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1]  Count: 0",               query4.ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1, MyComponent2]  Count: 0", query5.ToString());
        
        AreEqual(0, query1.Archetypes.Length);
        AreEqual(0, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<Position>();
        AreEqual(1, query1.Archetypes.Length);
        AreEqual(0, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<Rotation>();
        AreEqual(2, query1.Archetypes.Length);
        AreEqual(1, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<Scale3>();
        AreEqual(3, query1.Archetypes.Length);
        AreEqual(2, query2.Archetypes.Length);
        AreEqual(1, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<MyComponent1>();
        AreEqual(4, query1.Archetypes.Length);
        AreEqual(3, query2.Archetypes.Length);
        AreEqual(2, query3.Archetypes.Length);
        AreEqual(1, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<MyComponent2>();
        AreEqual(5, query1.Archetypes.Length);
        AreEqual(4, query2.Archetypes.Length);
        AreEqual(3, query3.Archetypes.Length);
        AreEqual(2, query4.Archetypes.Length);
        AreEqual(1, query5.Archetypes.Length);
    }
    
    [Test]
    public static void Test_Query_creation_Perf()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var sig     = Signature.Get<Position, Rotation>();
        var count   = 10; // 100_000_000 ~ #PC: 1.897 ms
        for (int n = 0; n < count; n++) {
            _ = store.Query(sig);
        }
    }
    
    [Test]
    public static void Test_Query_AsEntityList()
    {
        var store   = new EntityStore(PidType.RandomPids);
        for (int n = 0; n < 10; n++) {
            store.CreateEntity().AddComponent<Position>();
        }
        
        var query = store.Query<Position>();
        var count = 0;
        foreach (var entity in query.ToEntityList()) {
            entity.AddComponent<Rotation>(); // structural changes are allowed
            count++;
        }
        AreEqual(10, count);
        
        count = 0;
        foreach (var entity in query.Entities.ToEntityList()) {
            entity.AddComponent<Rotation>(); // structural changes are allowed
            count++;
        }
        AreEqual(10, count);
        
        count = 0;
        foreach (var entity in query.Entities.ToEntityList(new EntityList())) {
            entity.AddComponent<Rotation>(); // structural changes are allowed
            count++;
        }
        AreEqual(10, count);
    }
    
    [Test]
    public static void Test_Query_loop()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity2 = store.CreateEntity();
        entity2.AddComponent(new Position(1,2,3));
        entity2.AddComponent(new Rotation(4,5,6,7));
        
        var entity3  = store.CreateEntity();
        entity3.AddComponent(new Position(1,2,3));
        entity3.AddComponent(new Rotation(8, 8, 8, 8));
        entity3.AddComponent(new Scale3  (7, 7, 7));
        
        // var expect  = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 400 : 416;
        var sig     = Signature.Get<Position, Rotation>();
        var query   = store.Query(sig);
        _           = query.Archetypes; // create archetypes array to avoid creation in ChunkEnumerator to check allocation
        
        var start   = GetAllocatedBytes();
        foreach (var _ in query.Chunks) { }
        AssertNoAlloc(start);
        
        _ = query.Archetypes; // Note: force update of ArchetypeQuery.archetypes[] which resize the array if needed

        var chunkCount   = 0;
        AreEqual("QueryChunks[2]  Components: [Position, Rotation]", query.Chunks.ToString());
        foreach (var (_, _, _) in query.Chunks) { } // force one time allocations
        start = GetAllocatedBytes();
        foreach (var (position, rotation, _) in query.Chunks) {
            AreEqual(3, position[0].z);
            rotation[0].x = 42;
            chunkCount++;
        }
        AssertNoAlloc(start);
        AreEqual(2,  chunkCount);
        AreEqual(42, entity2.Rotation.x);
        
        chunkCount = 0;
        foreach (var (_, rotation, _) in query.Chunks) {
            switch (chunkCount++) {
                case 0:
                    rotation[0].x = 99;
                    break;
            }
        }
        AreEqual(2,  chunkCount);
        AreEqual(99, entity2.Rotation.x);
    }
    
    [Test]
    public static void Test_Query_Archetype_Entities()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var archetype = store.GetArchetype(ComponentTypes.Get<Position>());
        for (int n = 0; n < 10; n++) {
            archetype.CreateEntity();
        }
        AreEqual(10, archetype.Entities.Count);
        int count = 0;
        foreach (Entity _ in archetype.Entities) {
            count++;
        }
        AreEqual(10, count);
    }
    
    // [Test]
    public static void Test_Position_array_Perf() {
        var positions = new Position[10_000_000];
        for (int n = 0; n < 100; n++) {
            foreach (var position in positions) {
                _ = position;
            }
        }
    }
}

}

