using System;
using System.Diagnostics.CodeAnalysis;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public struct InternalTestTag  : ITag { }

[ExcludeFromCodeCoverage]
public static class Test_ComponentType
{
    [Test]
    public static void Test_ComponentSchema_Dependencies()
    {
        var schema = EntityStore.GetEntitySchema();
        AreEqual(4, schema.EngineDependants.Length);
        
        
        var e = Throws<InvalidOperationException>(() =>
        {
            schema.CheckStructIndex(typeof(string), schema.maxStructIndex);    
        });
        var expect = $"number of component types exceed EntityStore.maxStructIndex: {schema.maxStructIndex}";
        AreEqual(expect, e!.Message);
    }
    
    /// <summary> cover <see cref="SchemaUtils.CreateSchemaType"/> </summary>
    [Test]
    public static void Test_ComponentType_CreateSchemaType()
    {
        var schemaTypes = new SchemaTypes();
        var e = Throws<InvalidOperationException>(() => {
            SchemaUtils.CreateSchemaType(typeof(string), null, schemaTypes);
        });
        AreEqual("Cannot create SchemaType for Type: System.String", e!.Message);
        
        e = Throws<InvalidOperationException>(() => {
            SchemaUtils.CreateSchemaType(typeof(Guid), null, schemaTypes);
        });
        AreEqual("Cannot create SchemaType for Type: System.Guid", e!.Message);
    }
    
    [Test]
    public static void Test_ComponentType_DebugView()
    {
        var componentTypes = ComponentTypes.Get<Position, Rotation>();
        
        var debugView   = new ComponentTypesDebugView(componentTypes);
        var types       = debugView.Types;
        AreEqual(2,                 types.Length);
        AreEqual(typeof(Position),  types[0].Type);
        AreEqual(typeof(Rotation),  types[1].Type);
    }
}
