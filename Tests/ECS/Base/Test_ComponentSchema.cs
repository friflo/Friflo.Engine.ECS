using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base;

public static class Test_ComponentSchema
{
    [Test]
    public static void Test_EntityTags() {
        var schema      = EntityStore.GetComponentSchema();
        AreEqual(4,     schema.Tags.Length);
        
        var tags = schema.Tags;
        IsNull(tags[0]);
        for (int n = 1; n < tags.Length; n++) {
            var type = tags[n];
            AreEqual(n,                 type.tagIndex);
            AreEqual(0,                 type.structIndex);
            AreEqual(0,                 type.scriptIndex);
            AreEqual(ComponentKind.Tag, type.kind);
            IsNull(type.componentKey);
        }
        var testTagType = schema.TagTypeByType[typeof(TestTag)];
        AreEqual(3,                     schema.TagTypeByType.Count);
        AreEqual(typeof(TestTag),       testTagType.type);
        AreEqual("tag: [#TestTag]",     testTagType.ToString());
        
        testTagType = schema.GetTagType<TestTag>();
        AreEqual(3,                     schema.TagTypeByType.Count);
        AreEqual(typeof(TestTag),       testTagType.type);
        AreEqual("tag: [#TestTag]",     testTagType.ToString());
    }
    
    [Test]
    public static void Test_ComponentTypes()
    {
        var schema      = EntityStore.GetComponentSchema();
        var components  = schema.Components;
        var scripts   = schema.Scripts;
        
        AreEqual("components: 9  scripts: 5  entity tags: 3", schema.ToString());
        AreEqual(10,     components.Length);
        AreEqual( 6,     scripts.Length);
        
        AreEqual(14,    schema.ComponentTypeByKey.Count);
        AreEqual(14,    schema.ComponentTypeByType.Count);
        
        IsNull(components[0]);
        for (int n = 1; n < components.Length; n++) {
            var type = components[n];
            AreEqual(n, type.structIndex);
            AreEqual(0, type.tagIndex);
            AreEqual(0, type.scriptIndex);
            AreEqual(ComponentKind.Component, type.kind);
            NotNull (type.componentKey);
        }
        IsNull(scripts[0]);
        for (int n = 1; n < scripts.Length; n++) {
            var type = scripts[n];
            AreEqual(n, type.scriptIndex);
            AreEqual(0, type.tagIndex);
            AreEqual(0, type.structIndex);
            AreEqual(ComponentKind.Script, type.kind);
            NotNull (type.componentKey);
        }
        
        var posType = schema.ComponentTypeByKey["pos"];
        AreEqual(typeof(Position), posType.type);
        
        var testType = schema.ComponentTypeByKey["test"];
        AreEqual(typeof(TestComponent), testType.type);
        
        var myComponentType = schema.GetComponentType<MyComponent1>();
        AreEqual("my1",                                 myComponentType.componentKey);
        AreEqual("component: 'my1' [MyComponent1]",     myComponentType.ToString());
        
        var testComponentType = schema.GetScriptType<TestComponent>();
        AreEqual("test",                                testComponentType.componentKey);
        AreEqual("script: 'test' [*TestComponent]",     testComponentType.ToString());
        
        AreEqual(typeof(Position),  schema.ComponentTypeByKey["pos"].type);
        AreEqual("test",            schema.ComponentTypeByType[typeof(TestComponent)].componentKey);
    }
}
