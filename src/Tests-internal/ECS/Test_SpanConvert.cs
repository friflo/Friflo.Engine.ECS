using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

namespace Internal.ECS {

// ReSharper disable InconsistentNaming
public static class Test_SpanConvert
{
    [Test]
    public static void Test_Convert_Positions_to_Vectors()
    {
        var positions = new Position[] { new (1,1,1), new (2,2,2) };
        Span<Position> positionSpan = positions;
        var vectors = MemoryMarshal.Cast<Position, Vector3>(positionSpan);
        
        Mem.AreEqual(positions.Length, vectors.Length);
        Mem.AreEqual(positions[0].value, vectors[0]);
        Mem.AreEqual(positions[1].value, vectors[1]);
        
        var floats = MemoryMarshal.Cast<Position, float>(positionSpan);
        Mem.AreEqual(6, floats.Length);
        Mem.AreEqual(1, floats[0]);
        Mem.AreEqual(1, floats[1]);
        Mem.AreEqual(1, floats[2]);
        
        Mem.AreEqual(2, floats[3]);
        Mem.AreEqual(2, floats[4]);
        Mem.AreEqual(2, floats[5]);
        
        long count = 10; // 10_000_000_000L ~ #PC: 2447 ms
        for (long n = 0; n < count; n++) {
            MemoryMarshal.Cast<Position, Vector3>(positionSpan);
        }
    }
}

}