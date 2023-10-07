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
        AreEqual(1,     sig1.types.Length);
        AreSame(sig1,   Signature.Get<Position>());
        AreEqual("Signature: [Position]", sig1.ToString());
        
        var sig2 =      Signature.Get<Position, Rotation>();
        AreEqual(2,     sig2.types.Length);
        AreSame(sig2,   Signature.Get<Position, Rotation>());
        AreEqual("Signature: [Position, Rotation]", sig2.ToString());
        
        var sig3 =      Signature.Get<Position, Rotation, Scale3>();
        AreEqual(3,     sig3.types.Length);
        AreSame(sig3,   Signature.Get<Position, Rotation, Scale3>());
        AreEqual("Signature: [Position, Rotation, Scale3]", sig3.ToString());
        
        var sig4 =      Signature.Get<Position, Rotation, Scale3, EntityName>();
        AreEqual(4,     sig4.types.Length);
        AreSame(sig4,   Signature.Get<Position, Rotation, Scale3, EntityName>());
        AreEqual("Signature: [Position, Rotation, Scale3, EntityName]", sig4.ToString());

        var sig5 =      Signature.Get<Position, Rotation, Scale3, EntityName, MyComponent1>();
        AreEqual(5,     sig5.types.Length);
        AreSame(sig5,   Signature.Get<Position, Rotation, Scale3, EntityName, MyComponent1>());
        AreEqual("Signature: [Position, Rotation, Scale3, EntityName, MyComponent1]", sig5.ToString());
        
        // --- permute argument order
        var sig2_ =     Signature.Get<Rotation, Position>();
        AreNotSame(sig2, sig2_);
        AreEqual("Signature: [Rotation, Position]", sig2_.ToString());
        
        var sig3_ =     Signature.Get<Rotation, Position, Scale3>();
        AreNotSame(sig3, sig3_);
        AreEqual("Signature: [Rotation, Position, Scale3]", sig3_.ToString());
        
        var sig4_ =     Signature.Get<Rotation, Position, Scale3, EntityName>();
        AreNotSame(sig4, sig4_);
        AreEqual("Signature: [Rotation, Position, Scale3, EntityName]", sig4_.ToString());
        
        var sig5_ =     Signature.Get<Rotation, Position, Scale3, EntityName, MyComponent1>();
        AreNotSame(sig5, sig5_);
        AreEqual("Signature: [Rotation, Position, Scale3, EntityName, MyComponent1]", sig5_.ToString());
    }
}
