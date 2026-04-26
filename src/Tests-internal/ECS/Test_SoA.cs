using System;
using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_SoA
{
    private static unsafe void Assert32ByteAligned<T>(Span<T> span)
    {
        fixed (T* ptr = span)
        {
            bool isAligned = ((long)ptr & 31) == 0;
            Assert.IsTrue(isAligned);
        }
    }
    
    [Test]
    public static void Test_AoSFloat_Chunk_indexer_perf()
    {
        long repeat = 10; // 10_000_000_000;
        var store = new EntityStore();
        store.CreateEntity(new FloatComponent { value = 10 });
        var query = store.Query<FloatComponent>();
        int count = 0;
        foreach (var (pos, FloatComponent) in query.Chunks) {
            count++;
            for (long n = 0; n < repeat; n++) {
                _ = pos[0];    
            }
        }
        Assert.AreEqual(1, count);
    }
    
    const int Count = 10;
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_AoSFloat_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < Count; n++) {
            store.CreateEntity(new FloatComponent { value = n * 10 });
        }
        var query = store.Query<FloatComponent>();
        int count = 0;
        foreach (var (pos, FloatComponent) in query.Chunks) {
            count++;
            var lanes  = pos.GetComponentSpan();
            Assert32ByteAligned(lanes);
            for (int n = 0; n < Count; n++) {
                Assert.AreEqual(n * 10, pos[n].value);
            }
        }
        Assert.AreEqual(1, count);
    }
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_AoSVector4_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < Count; n++) {
            store.CreateEntity(new Pos4 { value = new Vector4(n * 10, n * 20, n * 30, n * 40) });
        }
        var query = store.Query<Pos4>();
        int count = 0;
        foreach (var (pos, Pos4) in query.Chunks) {
            count++;
            var lanes  = pos.GetComponentSpan();
            Assert32ByteAligned(lanes);
            for (int n = 0; n < Count; n++) {
                Assert.AreEqual(new Vector4(n * 10, n * 20, n * 30, n * 40), pos[n].value);
            }
        }
        Assert.AreEqual(1, count);
    }
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_AoSoAVector2_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < Count; n++) {
            store.CreateEntity(new Pos2AoSoA { value = new Vector2(n * 10, n * 20) });
        }
        var query = store.Query<Pos2AoSoA>();
        int count = 0;
        foreach (var (pos, entities) in query.Chunks) {
            count++;
            var lanes  = pos.GetLanesSoA();
            Assert32ByteAligned(lanes);
            for (int n = 0; n < Count; n++) {
                Assert.AreEqual(new Vector2(n * 10, n * 20), pos.GetAoSoA(n).value);
            }
        }
        Assert.AreEqual(1, count);
    }
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_AoSoAVector3_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < Count; n++) {
            store.CreateEntity(new Pos3AoSoA { value = new Vector3(n * 10, n * 20, n * 30) });
        }
        var query = store.Query<Pos3AoSoA>();
        int count = 0;
        foreach (var (pos, entities) in query.Chunks) {
            count++;
            var lanes  = pos.GetLanesSoA();
            Assert32ByteAligned(lanes);
            for (int n = 0; n < Count; n++) {
                Assert.AreEqual(new Vector3(n * 10, n * 20, n * 30), pos.GetAoSoA(n).value);
            }
        }
        Assert.AreEqual(1, count);
    }
    
    /// Test <see cref="Chunk{T}.GetLanesSoA"/>
    [Test]
    public static void Test_AoSoAVector4_Query_Lanes()
    {
        var store = new EntityStore();
        for (int n = 0; n < Count; n++) {
            store.CreateEntity(new Pos4AoSoA { value = new Vector4(n * 10, n * 20, n * 30, n * 40) });
        }
        var query = store.Query<Pos4AoSoA>();
        int count = 0;
        foreach (var (pos, entities) in query.Chunks) {
            count++;
            var lanes  = pos.GetLanesSoA();
            Assert32ByteAligned(lanes);
            for (int n = 0; n < Count; n++) {
                Assert.AreEqual(new Vector4(n * 10, n * 20, n * 30, n * 40), pos.GetAoSoA(n).value);
            }
        }
        Assert.AreEqual(1, count);
    }
    
}