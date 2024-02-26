using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {

public static class Test_Components
{
    [Test]
    public static void Test_Components_Equality()
    {
        IsTrue  (new Position()         == default);
        IsFalse (new Position()         != default);
        IsTrue  (new Position().Equals(default));
        AreEqual("1, 2, 3", new Position(1, 2, 3).ToString());
        
        IsTrue  (new Rotation()         == default);
        IsFalse (new Rotation()         != default);
        IsTrue  (new Rotation().Equals(default));
        AreEqual("1, 2, 3, 4", new Rotation(1, 2, 3, 4).ToString());
        
        IsTrue  (new Scale3()           == default);
        IsFalse (new Scale3()           != default);
        IsTrue  (new Scale3().Equals(default));
        AreEqual("1, 2, 3", new Scale3(1, 2, 3).ToString());
    }
}

}