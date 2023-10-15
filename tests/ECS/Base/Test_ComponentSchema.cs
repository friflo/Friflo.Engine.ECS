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
            AreEqual(0,                 type.classIndex);
            AreEqual(ComponentKind.Tag, type.kind);
            IsNull(type.componentKey);
        }
        var testTagType = schema.TagTypeByType[typeof(TestTag)];
        AreEqual(3,                     schema.TagTypeByType.Count);
        AreEqual(typeof(TestTag),       testTagType.type);
        AreEqual("tag: [#TestTag]",     testTagType.ToString());
    }
    
    [Test]
    public static void Test_ComponentTypes()
    {
        var schema  = EntityStore.GetComponentSchema();
        var structs = schema.Structs;
        var classes = schema.Classes;
        
        AreEqual("struct components: 6  class components: 5  entity tags: 3", schema.ToString());
        AreEqual(7,     structs.Length);
        AreEqual(6,     classes.Length);
        
        AreEqual(11,     schema.ComponentTypeByKey.Count);
        AreEqual(11,     schema.ComponentTypeByType.Count);
        
        IsNull(structs[0]);
        for (int n = 1; n < structs.Length; n++) {
            var type = structs[n];
            AreEqual(n, type.structIndex);
            AreEqual(0, type.tagIndex);
            AreEqual(0, type.classIndex);
            AreEqual(ComponentKind.Struct, type.kind);
            NotNull (type.componentKey);
        }
        IsNull(classes[0]);
        for (int n = 1; n < classes.Length; n++) {
            var type = classes[n];
            AreEqual(n, type.classIndex);
            AreEqual(0, type.tagIndex);
            AreEqual(0, type.structIndex);
            AreEqual(ComponentKind.Class, type.kind);
            NotNull (type.componentKey);
        }
        
        var posType = schema.GetComponentTypeByKey("pos");
        AreEqual(typeof(Position), posType.type);
        
        var testType = schema.GetComponentTypeByKey("test");
        AreEqual(typeof(TestComponent), testType.type);
        
        var myComponentType = schema.GetStructComponentType<MyComponent1>();
        AreEqual("my1",                             myComponentType.componentKey);
        AreEqual("struct component: [MyComponent1]",  myComponentType.ToString());
        
        var testComponentType = schema.GetClassComponentType<TestComponent>();
        AreEqual("test",                            testComponentType.componentKey);
        AreEqual("class component: [*TestComponent]", testComponentType.ToString());
        
        AreEqual(typeof(Position),  schema.ComponentTypeByKey["pos"].type);
        AreEqual("test",            schema.ComponentTypeByType[typeof(TestComponent)].componentKey);
    }
}
