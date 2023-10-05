using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Signature_POC
{
    [Test]
    public static void Test_Signature()
    {
        var sig1 =      Signature.Get<Position>();
        AreEqual(1,     sig1.ComponentTypes.Length);
        AreSame(sig1,   Signature.Get<Position>());
        AreEqual("[Position]", sig1.ToString());
        
        var sig2 =      Signature.Get<Position, Rotation>();
        AreEqual(2,     sig2.ComponentTypes.Length);
        AreSame(sig2,   Signature.Get<Position, Rotation>());
        AreEqual("[Position, Rotation]", sig2.ToString());
        
        var sig3 =      Signature.Get<Position, Rotation, Scale3>();
        AreEqual(3,     sig3.ComponentTypes.Length);
        AreSame(sig3,   Signature.Get<Position, Rotation, Scale3>());
        AreEqual("[Position, Rotation, Scale3]", sig3.ToString());
        
        var sig4 =      Signature.Get<Position, Rotation, Scale3, EntityName>();
        AreEqual(4,     sig4.ComponentTypes.Length);
        AreSame(sig4,   Signature.Get<Position, Rotation, Scale3, EntityName>());
        AreEqual("[Position, Rotation, Scale3, EntityName]", sig4.ToString());

        var sig5 =      Signature.Get<Position, Rotation, Scale3, EntityName, MyComponent1>();
        AreEqual(5,     sig5.ComponentTypes.Length);
        AreSame(sig5,   Signature.Get<Position, Rotation, Scale3, EntityName, MyComponent1>());
        AreEqual("[Position, Rotation, Scale3, EntityName, MyComponent1]", sig5.ToString());

    }
}
