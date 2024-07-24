using System;
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
        
        var schema      = aot.CreateSchema();
        var dependants  = schema.EngineDependants;
        AreEqual(2, dependants.Length);
        
        var engine  = dependants[0];
        AreEqual("Friflo.Engine.ECS",   engine.AssemblyName);
        AreEqual(9,                     engine.Types.Length);
        
        var tests = dependants[1];
        AreEqual("Tests",   tests.AssemblyName);
        AreEqual(3,         tests.Types.Length);
        
        var createdSchema = NativeAOT.GetSchema();
        AreSame(schema, createdSchema);
        
        var e = Throws<InvalidOperationException>(() => {
            aot.CreateSchema();
        });
        AreEqual("EntitySchema already created", e!.Message);
    }
}

}