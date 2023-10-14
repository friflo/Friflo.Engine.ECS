using System.Runtime.InteropServices;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_sizeof
{
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type ('EntityNode')
    [Test]
    public static unsafe void Test_sizeof_EntityNode() {
        var size = sizeof(EntityNode);
        AreEqual(48, size);
    }
    
    [Test]
    public static unsafe void Test_sizeof_BitSet() {
        var size = sizeof(BitSet);
        AreEqual(32, size);
    }
    
    [Test]
    public static unsafe void Test_sizeof_Tags() {
        var size = sizeof(Tags);
        AreEqual(32, size);
    }
    
    [Test]
    public static unsafe void Test_sizeof_ArchetypeStructs() {
        var size = sizeof(ArchetypeStructs);
        AreEqual(32, size);
    }
        
    [Test]
    public static void Test_sizeof_StructIndexes() {
        var type = typeof(SignatureIndexes);
        var size = Marshal.SizeOf(type!);
        AreEqual(24, size);
    }
    
    [Test]
    public static unsafe void Test_Math_sizeof() {
        var size = sizeof(Position);
        AreEqual(12, size);
        
        size = sizeof(Rotation);
        AreEqual(16, size);
        
        size = sizeof(Scale3);
        AreEqual(12, size);
    }
}