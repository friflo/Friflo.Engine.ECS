using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.Internal.ECS;

public static class Test_ArchetypeStructs
{
    [Test]
    public static void Test_ArchetypeMask_invalid_constructor()
    {
        object signatureIndexes = new SignatureIndexes();
        signatureIndexes.SetInternalField(nameof(SignatureIndexes.length), 6);
        Throws<IndexOutOfRangeException>(() => {
            _ = new ArchetypeStructs ((SignatureIndexes)signatureIndexes);
        });
    }
    
    [Test]
    public static void Test_ArchetypeKey()
    {
        var store       = new EntityStore();
        var posType     = store.GetArchetype(Signature.Get<Position>());
        var posRotType  = store.GetArchetype(Signature.Get<Position, Rotation>());
        
        AreEqual(1,                             posType.Structs.Count);
        AreEqual(2,                             posRotType.Structs.Count);
        AreEqual("Key: [Position]",             posType.key.ToString());
        AreEqual("Key: [Position, Rotation]",   posRotType.key.ToString());
    }
}

