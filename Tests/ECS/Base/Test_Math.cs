using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {

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
}

}

