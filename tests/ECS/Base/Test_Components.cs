using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;
// ReSharper disable EqualExpressionComparison


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base;

public static class Test_Components
{
    [Test]
    public static void Test_Components_Equality()
    {
        IsTrue  (new Position()         == default);
        IsFalse (new Position()         != default);
        IsTrue  (new Position().Equals(default));
        
        IsTrue  (new Rotation()         == default);
        IsFalse (new Rotation()         != default);
        IsTrue  (new Rotation().Equals(default));
        
        IsTrue  (new Scale3()           == default);
        IsFalse (new Scale3()           != default);
        IsTrue  (new Scale3().Equals(default));

    }
}