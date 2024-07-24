using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_Signature
{
    [Test]
    public static void Test_Signature_Get()
    {
        var sig1 =      Signature.Get<Position>();
        AreEqual(1,     sig1.ComponentCount);
        AreEqual("Signature: [Position]", sig1.ToString());
        
        var sig2 =      Signature.Get<Position, Rotation>();
        AreEqual(2,     sig2.ComponentCount);
        AreEqual("Signature: [Position, Rotation]", sig2.ToString());
        
        var sig3 =      Signature.Get<Position, Rotation, Scale3>();
        AreEqual(3,     sig3.ComponentCount);
        AreEqual("Signature: [Position, Rotation, Scale3]", sig3.ToString());
        
        var sig4 =      Signature.Get<Position, Rotation, Scale3, EntityName>();
        AreEqual(4,     sig4.ComponentCount);
        AreEqual("Signature: [Position, Rotation, Scale3, EntityName]", sig4.ToString());

        var sig5 =      Signature.Get<Position, Rotation, Scale3, EntityName, MyComponent1>();
        AreEqual(5,     sig5.ComponentCount);
        AreEqual("Signature: [Position, Rotation, Scale3, EntityName, MyComponent1]", sig5.ToString());
        
        // --- permute argument order
        var sig2_ =     Signature.Get<Rotation, Position>();
        AreEqual("Signature: [Rotation, Position]", sig2_.ToString());
        
        var sig3_ =     Signature.Get<Rotation, Position, Scale3>();
        AreEqual("Signature: [Rotation, Position, Scale3]", sig3_.ToString());
        
        var sig4_ =     Signature.Get<Rotation, Position, Scale3, EntityName>();
        AreEqual("Signature: [Rotation, Position, Scale3, EntityName]", sig4_.ToString());
        
        var sig5_ =     Signature.Get<Rotation, Position, Scale3, EntityName, MyComponent1>();
        AreEqual("Signature: [Rotation, Position, Scale3, EntityName, MyComponent1]", sig5_.ToString());
    }
    
    [Test]
    public static void Test_Signature_GetComponentTypes() {
        var ct1 = ComponentTypes.Get<Position>()                                            .ToString();
        var ct2 = ComponentTypes.Get<Position, Rotation>()                                  .ToString();
        var ct3 = ComponentTypes.Get<Position, Rotation, Scale3>()                          .ToString();
        var ct4 = ComponentTypes.Get<Position, Rotation, Scale3, EntityName>()              .ToString();
        var ct5 = ComponentTypes.Get<Position, Rotation, Scale3, EntityName, MyComponent1>().ToString();
        
        var s1 = Signature.Get<Position>();
        var s2 = Signature.Get<Position, Rotation>();
        var s3 = Signature.Get<Position, Rotation, Scale3>();
        var s4 = Signature.Get<Position, Rotation, Scale3, EntityName>();
        var s5 = Signature.Get<Position, Rotation, Scale3, EntityName, MyComponent1>();
        
        AreEqual(s1.ComponentTypes.ToString(), ct1);
        AreEqual(s2.ComponentTypes.ToString(), ct2);
        AreEqual(s3.ComponentTypes.ToString(), ct3);
        AreEqual(s4.ComponentTypes.ToString(), ct4);
        AreEqual(s5.ComponentTypes.ToString(), ct5);
    }
}

}