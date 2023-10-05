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
        var sig1 =      Signature.Create<Position>();
        AreEqual(1,     sig1.ComponentTypes.Length);
        AreSame(sig1,   Signature.Create<Position>());
        AreEqual("[Position]", sig1.ToString());
        
        var sig2 =      Signature.Create<Position, Rotation>();
        AreEqual(2,     sig2.ComponentTypes.Length);
        AreSame(sig2,   Signature.Create<Position, Rotation>());
        AreEqual("[Position, Rotation]", sig2.ToString());
        
        var sig3 =      Signature.Create<Position, Rotation, Scale3>();
        AreEqual(3,     sig3.ComponentTypes.Length);
        AreSame(sig3,   Signature.Create<Position, Rotation, Scale3>());
        AreEqual("[Position, Rotation, Scale3]", sig3.ToString());
    }
}
