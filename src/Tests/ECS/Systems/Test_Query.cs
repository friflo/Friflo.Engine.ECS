using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Systems {

public static class Test_Query
{
    private const long Count = 10; // 10_000_000;
        
    [Test]
    public static void Test_Query_arg_count_1()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position>());
        for (int n = 1; n <= 1000; n++) {
            var child = archetype.CreateEntity();
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 1) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Scale3, Transform, TreeNode, MyComponent1]  entities: 1", chunk.ToString());
                var positions = chunk.Chunk1;
                Mem.AreEqual("Position[1]", positions.ToString());
                Mem.AreEqual(1, positions[0].x);
                var e = Assert.Throws<IndexOutOfRangeException>(() => {
                    _ = positions[1];
                });
                Mem.AreEqual("Index was outside the bounds of the array.", e!.Message);
            }
        }
        Mem.AreEqual(2, chunkCount);
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ #PC: 679 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    

    [Test]
    public static void Test_Query_arg_count_2()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 2 });
        
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position, Rotation>());
        for (int n = 2; n <= 1000; n++) {
            var child = archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 1) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Scale3, Transform, TreeNode, MyComponent1]  entities: 1", chunk.ToString());
            }
        }
        Mem.AreEqual(2, chunkCount);
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ #PC: 670 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }

    [Test]
    public static void Test_Query_arg_count_3()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 3 });
        
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, EntityName>());
        for (int n = 2; n <= 1000; n++) {
            var child = archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, EntityName>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(999, chunk.Length);
                Mem.AreEqual("Chunks[999]    Archetype: [EntityName, Position, Rotation]  entities: 999", chunk.ToString());
            }
        }
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ #PC: 845 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Query_arg_count_4()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 4 });
        
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, Scale3, EntityName>());
        for (int n = 2; n <= 1000; n++) {
            var child = archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, Scale3, EntityName>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 1) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Scale3, Transform, TreeNode, MyComponent1]  entities: 1", chunk.ToString());
            }
        }
        Mem.AreEqual(2, chunkCount);
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ #PC: 966 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    
    [Test]
    public static void Test_Query_arg_count_5()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        root.AddScript(new SystemSet { argCount = 5 });
        
        var archetype   = store.GetArchetype(ComponentTypes.Get<Position, Rotation, Scale3, EntityName>());
        for (int n = 2; n <= 1000; n++) {
            var child = archetype.CreateEntity();
            child.Position      = new Position(n, 0, 0);
            child.Rotation      = new Rotation(n, 0, 0, 0);
            child.Scale3        = new Scale3  (n, 0, 0);
            child.Name.value    = "child";
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<Position, Rotation, Scale3, Transform, EntityName>();
        int chunkCount = 0;
        foreach (var chunk in query.Chunks) {
            if (chunkCount++ == 0) {
                Mem.AreEqual(1, chunk.Length);
                Mem.AreEqual("Chunks[1]    Archetype: [EntityName, Position, Rotation, Scale3, Transform, TreeNode, MyComponent1]  entities: 1", chunk.ToString());
            }
        }
        Mem.AreEqual(1, chunkCount);
        AssertChunkExtensions(query);
        
        // --- run perf
        var start = Mem.GetAllocatedBytes();
        // 10_000_000 ~ #PC: 1078 ms
        for (long n = 0; n < Count; n++) {
            foreach (var (_, _, _, _, _, _) in query.Chunks) { }
        }
        Mem.AssertNoAlloc(start);
    }
    
    private static void AssertChunkExtensions(ArchetypeQuery<Position, Rotation, Scale3, Transform, EntityName> query) {

        foreach (var chunk in query.Chunks) {
            var length = chunk.Entities.Length;
            
            Mem.AreEqual(length, chunk.Chunk1.     AsSpanVector3().Length);
            Mem.AreEqual(length, chunk.Chunk1.Span.AsSpanVector3().Length);
            //
            Mem.AreEqual(length, chunk.Chunk2.     AsSpanQuaternion().Length);
            Mem.AreEqual(length, chunk.Chunk2.Span.AsSpanQuaternion().Length);
            //
            Mem.AreEqual(length, chunk.Chunk3.     AsSpanVector3().Length);
            Mem.AreEqual(length, chunk.Chunk3.Span.AsSpanVector3().Length);
            //
            Mem.AreEqual(length, chunk.Chunk4.     AsSpanMatrix4x4().Length);
            Mem.AreEqual(length, chunk.Chunk4.Span.AsSpanMatrix4x4().Length);
        }
    }
    
    private static EntityStore SetupTestStore() {
        var store   = new EntityStore();
        
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("root"));
        root.AddComponent(new Position(1, 0, 0));
        root.AddComponent<Rotation>();
        root.AddComponent<Transform>();
        root.AddComponent<Scale3>();
        root.AddComponent<MyComponent1>();
        store.SetStoreRoot(root);
        return store;
    }
    
    [Test]
    public static void Test_Query_Chunk_StepVector()
    {
        var store = SetupTestStore();
        var root  = store.StoreRoot;
        
        var archetype   = store.GetArchetype(ComponentTypes.Get<ByteComponent, MyComponent1>());
        for (int n = 0; n < 32; n++) {
            var child = archetype.CreateEntity();
            root.AddChild(child);
        }
        // --- force one time allocations
        var  query = store.Query<ByteComponent, MyComponent1>();
        foreach (var (byteComponent, intComponent, _) in query.Chunks) {
            Mem.AreEqual(16, byteComponent.StepSpan128);
            Mem.AreEqual(32, byteComponent.StepSpan256);
            Mem.AreEqual(64, byteComponent.StepSpan512);
            
            Mem.AreEqual(4,  intComponent.StepSpan128);
            Mem.AreEqual(8,  intComponent.StepSpan256);
            Mem.AreEqual(16, intComponent.StepSpan512);
        }
    }
    
    [Test]
    public static void Test_Query_Chunk_Padding() {
        
        var store       = new EntityStore();
        var archetype   = store.GetArchetype(ComponentTypes.Get<ByteComponent>());
        var query       = store.Query<ByteComponent>();
        for (int n = 0; n < 200; n++) {
            foreach (var (components, _) in query.Chunks)
            {
                var span128 = components.AsSpan128<byte>();
                switch (n) {
                    case 0:                     Mem.AreEqual(  0, span128.Length);  break;
                    case >   0   and <=  16:    Mem.AreEqual( 16, span128.Length);  break;
                    case >  16   and <=  32:    Mem.AreEqual( 32, span128.Length);  break;
                    case >  32   and <=  48:    Mem.AreEqual( 48, span128.Length);  break;
                    case >  48   and <=  64:    Mem.AreEqual( 64, span128.Length);  break;
                }
                
                var span256 = components.AsSpan256<byte>();
                // Console.WriteLine($"components - Length: {components.Length}, AsSpan256<byte>.Length: {span256.Length}");
                switch (n) {
                    case 0:                     Mem.AreEqual(  0, span256.Length);  break;
                    case >   0   and <=  32:    Mem.AreEqual( 32, span256.Length);  break;
                    case >  32   and <=  64:    Mem.AreEqual( 64, span256.Length);  break;
                    case >  64   and <=  96:    Mem.AreEqual( 96, span256.Length);  break;
                    case >  96   and <= 128:    Mem.AreEqual(128, span256.Length);  break;
                }
                
                var span512 = components.AsSpan512<byte>();
                switch (n) {
                    case 0:                     Mem.AreEqual(  0, span512.Length);  break;
                    case >   0   and <=  64:    Mem.AreEqual( 64, span512.Length);  break;
                    case >  64   and <= 128:    Mem.AreEqual(128, span512.Length);  break;
                    case >  128  and <= 192:    Mem.AreEqual(192, span512.Length);  break;
                    case >  192:                Mem.AreEqual(256, span512.Length);  break;
                }
            }
            archetype.CreateEntity();
        }
    }
}

}

