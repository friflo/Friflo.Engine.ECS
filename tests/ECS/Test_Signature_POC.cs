// ReSharper disable InconsistentNaming

using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;

namespace Tests.ECS;

public static class Test_Signature_POC
{
    [Test]
    public static void Test_Signature_API() {
        var pos     = Sig.Create<Position>();
        var posRot  = Sig.Create<Position, Rotation>();
        
        var store = new TestStore();
        
        store.Query(Sig.Create<Position>());
        store.Query(pos);
        store.Query(posRot);
    }
}

public class TestStore
{
    public ArchetypeQuery Query(Sig sig) {
        return null;
    }
}

public class Sig
{
    public static Sig Create<T>()
        where T : struct
    {
        return null;
    }
    
    public static Sig Create<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        return null;
    }
}