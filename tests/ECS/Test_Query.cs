using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Query
{
    [Test]
    public static void Test_SignatureTypes()
    {
        var types           = new SignatureTypeSet();
        AreEqual("TypeSet: []",                     types.ToString());
        

        var sig1            = Signature.Get<Position>();
        AreEqual("Signature: [Position]",           sig1.ToString());
        AreEqual("TypeSet: [Position]",             sig1.types.ToString());
        
        var sig2            = Signature.Get<Position, Rotation>();
        AreEqual("Signature: [Position, Rotation]", sig2.ToString());

        int count = 0;
        foreach (var _ in sig2.types) {
            count++;    
        }
        AreEqual(2, count);
    }
    
    [Test]
    public static void Test_Signature_Get_Mem()
    {
        Signature.Get<Position>();  // force one time allocation
        
        var start   = Mem.GetAllocatedBytes();
        
        var sig1 = Signature.Get<Position>();
        var sig2 = Signature.Get<Position, Rotation>();
        var sig3 = Signature.Get<Position, Rotation, Scale3>();
        var sig4 = Signature.Get<Position, Rotation, Scale3, MyComponent1>();
        var sig5 = Signature.Get<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        
        Mem.AssertNoAlloc(start);
        
        AreEqual("Mask: [Position]",                                                sig1.structs.ToString());
        AreEqual("Mask: [Position, Rotation]",                                      sig2.structs.ToString());
        AreEqual("Mask: [Position, Rotation, Scale3]",                              sig3.structs.ToString());
        AreEqual("Mask: [Position, Rotation, Scale3, MyComponent1]",                sig4.structs.ToString());
        AreEqual("Mask: [Position, Rotation, Scale3, MyComponent1, MyComponent2]",  sig5.structs.ToString());
    }
    
    [Test]
    public static void Test_Signature_Query()
    {
        var store   = new EntityStore();
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
        
        AreEqual("Query: [Position]", query1.ToString());
        AreEqual("Query: [Position, Rotation]", query2.ToString());
        AreEqual("Query: [Position, Rotation, Scale3]", query3.ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1]", query4.ToString());
        AreEqual("Query: [Position, Rotation, Scale3, MyComponent1, MyComponent2]", query5.ToString());
        
    /*  AreSame(query1, store.Query(sig1));
        AreSame(query2, store.Query(sig2));
        AreSame(query3, store.Query(sig3));
        AreSame(query4, store.Query(sig4));
        AreSame(query5, store.Query(sig5)); */
        
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
    public static void Test_Query_ForEach()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new Position(1,2,3));
        entity.AddComponent(new Rotation(4,5,6,7));
        
        var entity3  = store.CreateEntity();
        entity3.AddComponent(new Position(1,2,3));
        entity3.AddComponent(new Rotation(8, 8, 8, 8));
        entity3.AddComponent(new Scale3  (7, 7, 7));
        
        var sig     = Signature.Get<Position, Rotation>();
        var query   = store.Query(sig);
        var count   = 0;
        var forEach = query.ForEach((position, rotation) => {
            count++;
            AreEqual(3, position.Value.z);
            rotation.Value.x = 42;
        });
        AreEqual("ForEach: [Position, Rotation]", forEach.ToString());
        forEach.Run();
        AreEqual(2,     count);
        AreEqual(42,    entity.Rotation.x);
    }
    
    [Test]
    public static void Test_Query_ForEach_RO()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new Position(1,2,3));
        entity.AddComponent(new Rotation(4,5,6,7));
        
        var sig     = Signature.Get<Position, Rotation>();
        var query   = store.Query(sig).ReadOnly<Position>();
        var count   = 0;
        var forEach = query.ForEach((position, rotation) => {
            // ReSharper disable once AccessToModifiedClosure
            count++;
            position.Value.x = 42;
        });
        var start = Mem.GetAllocatedBytes();
        forEach.Run();
        Mem.AssertNoAlloc(start);
        AreEqual(1,     count);
        AreEqual(1,     entity.Position.x);
        //
        count   = 0;
        start = Mem.GetAllocatedBytes();
        forEach.Run();
        
        Mem.AssertNoAlloc(start);
        AreEqual(1,     count);
        AreEqual(1,     entity.Position.x);
    }
    
    [Test]
    public static void Test_Query_loop()
    {
        var store   = new EntityStore();
        var entity2  = store.CreateEntity();
        entity2.AddComponent(new Position(1,2,3));
        entity2.AddComponent(new Rotation(4,5,6,7));
        
        var entity3  = store.CreateEntity();
        entity3.AddComponent(new Position(1,2,3));
        entity3.AddComponent(new Rotation(8, 8, 8, 8));
        entity3.AddComponent(new Scale3  (7, 7, 7));
        
        var sig     = Signature.Get<Position, Rotation>();
        var query   = store.Query(sig);
        var count   = 0;
        foreach (var (position, rotation) in query) {
            AreEqual(3, position.Value.z);
            rotation.Value.x = 42;
            count++;
            AreEqual("1, 2, 3", position.ToString());
        }
        AreEqual(2,  count);
        AreEqual(42, entity2.Rotation.x);
        
        var chunkCount   = 0;
        AreEqual("Chunks: [Position, Rotation]", query.Chunks.ToString());
        foreach (var (position, rotation) in query.Chunks) {
            AreEqual(3, position.Values[0].z);
            rotation.Values[0].x = 42;
            chunkCount++;
        }
        AreEqual(2,  chunkCount);
        AreEqual(42, entity2.Rotation.x);
    }
    
    [Test]
    public static void Test_Query_loop_Mem()
    {
        var store   = new EntityStore();
        var entity2  = store.CreateEntity();
        entity2.AddComponent(new Position(1,2,3));
        entity2.AddComponent(new Rotation(4,5,6,7));
        
        var entity3  = store.CreateEntity();
        entity3.AddComponent(new Position(1,2,3));
        entity3.AddComponent(new Rotation(8, 8, 8, 8));
        entity3.AddComponent(new Scale3  (7, 7, 7));
        
        var sig     = Signature.Get<Position, Rotation>();
        var start   = Mem.GetAllocatedBytes();
        var query   = store.Query(sig);
        Mem.AssertAlloc(start, 200);
        
        _ = query.Archetypes; // Note: force update of ArchetypeQuery.archetypes[] which resize the array if needed
        
        start       = Mem.GetAllocatedBytes();
        var count   = 0;
        foreach (var (position, rotation) in query) {
            if (3f != position.Value.z) {
                Fail($"Expect 3. was: {position.Value.z}");
            }
            rotation.Value.x = 42;
            count++;
        }
        Mem.AssertNoAlloc(start);
        AreEqual(2,  count);
        AreEqual(42, entity2.Rotation.x);
        
        var chunkCount   = 0;
        start = Mem.GetAllocatedBytes();
        foreach (var (position, rotation) in query.Chunks) {
            rotation.Values[0].x = 42;
            chunkCount++;
        }
        Mem.AssertNoAlloc(start);
        AreEqual(2,  chunkCount);
        AreEqual(42, entity2.Rotation.x);
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

