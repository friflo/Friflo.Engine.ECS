using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Signature
{
    [Ignore("missing dependency")][Test]
    public static void Test_ComponentSchema_Dependencies()
    {
        var schema  = EntityStore.GetComponentSchema();
        AreEqual(3, schema.Dependencies.Length);
    }
    
    [Test]
    public static void Test_SignatureIndexes()
    {
        var parameters = new object[] { 6, 0, 0, 0, 0, 0 };
        Throws<IndexOutOfRangeException>(() => {
            _ = new SignatureIndexes (6);
        });
        
        var indexes = new SignatureIndexes(0);
        Throws<IndexOutOfRangeException>(() => {
            indexes.GetStructIndex(5);
        });
        var schema  = EntityStore.GetComponentSchema();
        var posType = schema.GetStructComponentType<Position>();
        
        indexes = new SignatureIndexes(1, posType.structIndex);
        AreEqual("StructIndexes: [Position]", indexes.ToString());
    }
}
