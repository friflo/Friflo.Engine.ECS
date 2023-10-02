using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Math
{
    [Test]
    public static void Test_Math_Position() {
        var pos = new Position(1, 2, 3);
        AreEqual(1, pos.value.X);
        AreEqual(2, pos.value.Y);
        AreEqual(3, pos.value.Z);
    }
    
    [Test]
    public static void Test_Math_Rotation() {
        var rot = new Rotation(1, 2, 3, 4);
        AreEqual(1, rot.value.X);
        AreEqual(2, rot.value.Y);
        AreEqual(3, rot.value.Z);
        AreEqual(4, rot.value.W);
    }
    
    [Test]
    public static void Test_Math_Scale3() {
        var scale3 = new Scale3(1, 2, 3);
        AreEqual(1, scale3.value.X);
        AreEqual(2, scale3.value.Y);
        AreEqual(3, scale3.value.Z);
    }

    // #pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type ('EntityNode')
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

