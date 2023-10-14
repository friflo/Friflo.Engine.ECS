using System;
using System.Diagnostics.CodeAnalysis;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public struct InternalTestTag  : IEntityTag { }

[ExcludeFromCodeCoverage]
public static class Test_Signature
{
    [Test]
    public static void Test_ComponentSchema_Dependencies()
    {
        ComponentSchema schema = EntityStore.GetComponentSchema();
        AreEqual(3, schema.Dependencies.Length);
        
        
        var e = Throws<InvalidOperationException>(() =>
        {
            schema.GetStructType(schema.maxStructIndex, typeof(string));    
        });
        var expect = $"number of structs exceed EntityStore.maxStructIndex: {schema.maxStructIndex}";
        AreEqual(expect, e!.Message);
    }
    
    [Test]
    public static void Test_ComponentType_Exceptions()
    {
        ComponentType componentTYpe = new TagType(typeof(string), 0);
        Throws<InvalidOperationException>(() => {
            componentTYpe.ReadClassComponent(null, default, null);
        });
        Throws<InvalidOperationException>(() => {
            componentTYpe.CreateHeap(0);
        });
    }
    
    [Test]
    public static void Test_ComponentType_MissingAttribute()
    {
        var e = Throws<InvalidOperationException>(() => {
            ComponentUtils.RegisterComponentType(typeof(string), null, null, null, null);
        });
        AreEqual("missing expected attribute. Type: System.String", e!.Message);
    }
    
    [Test]
    public static void Test_SignatureIndexes()
    {
        Throws<IndexOutOfRangeException>(() => {
            _ = new SignatureIndexes (6);
        });
        
        var indexes = new SignatureIndexes(0);
        Throws<IndexOutOfRangeException>(() => {
            indexes.GetStructIndex(0);
        });
        var schema  = EntityStore.GetComponentSchema();
        var posType = schema.GetStructComponentType<Position>();
        
        indexes = new SignatureIndexes(1, posType.structIndex);
        AreEqual("StructIndexes: [Position]", indexes.ToString());
    }
}
