using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

namespace Internal.ECS;

// ReSharper disable InconsistentNaming
public static class Test_SpanConvert
{
    [Test]
    public static void Test_Convert_Position_to_Vectors() {
        Span<Position> positions = new Position[] { new Position(1,1,1), new Position(2,2,2) };
        var vectors = MemoryMarshal.Cast<Position, Vector3>(positions);
        
        Mem.AreEqual(positions.Length, vectors.Length);
        Mem.AreEqual(positions[0].value, vectors[0]);
        Mem.AreEqual(positions[1].value, vectors[1]);
    }
}