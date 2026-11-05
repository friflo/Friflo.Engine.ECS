using System;
using System.IO;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_NativeAOT
{
    [Test]
    public static void Test_NativeAOT_CreateSchema() {

        var aot = new NativeAOT();
        
        aot.RegisterComponent<MyComponent1>();
        aot.RegisterComponent<MyComponent1>();  // register again
        
        aot.RegisterTag<TestTag>();
        aot.RegisterTag<TestTag>();             // register again
        
        aot.RegisterScript<TestScript1>();
        aot.RegisterScript<TestScript1>();      // register again
        
        // process uses the schema created by reflection. CreateSchema() must not create a second schema
        var schema = aot.CreateSchema();
        AreSame(EntityStore.GetEntitySchema(), schema);
        AreSame(schema, aot.CreateSchema());
        IsTrue(schema.ComponentTypeByType.ContainsKey(typeof(MyComponent1)));
        IsTrue(schema.TagTypeByType.ContainsKey(typeof(TestTag)));
    }
    
    [Test]
    public static void Test_NativeAOT_DisableReflectionSchema()
    {
        IsTrue(SchemaUtils.RegisterComponentTypesByReflection());
        try {
            AppContext.SetSwitch("Friflo.Engine.ECS.DisableReflectionSchema", true);
            IsFalse(SchemaUtils.RegisterComponentTypesByReflection());
        }
        finally {
            AppContext.SetSwitch("Friflo.Engine.ECS.DisableReflectionSchema", false);
        }
        IsTrue(SchemaUtils.RegisterComponentTypesByReflection());
    }
    
    [Test]
    public static void Test_NativeAOT_DisableSchemaLog()
    {
        var consoleOut  = Console.Out;
        var writer      = new StringWriter();
        Console.SetOut(writer);
        try {
            SchemaUtils.Log("log enabled");
            AppContext.SetSwitch("Friflo.Engine.ECS.DisableSchemaLog", true);
            SchemaUtils.Log("log disabled");
        }
        finally {
            AppContext.SetSwitch("Friflo.Engine.ECS.DisableSchemaLog", false);
            Console.SetOut(consoleOut);
        }
        AreEqual("log enabled" + Environment.NewLine, writer.ToString());
    }
}

}