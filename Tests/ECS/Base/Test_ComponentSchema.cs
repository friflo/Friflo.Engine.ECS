using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base;

public static class Test_ComponentSchema
{
    [Test]
    public static void Test_EntityTags() {
        var schema      = EntityStore.GetEntitySchema();
        AreEqual(4,     schema.Tags.Length);
        
        var tags = schema.Tags;
        IsNull(tags[0]);
        for (int n = 1; n < tags.Length; n++) {
            var type = tags[n];
            AreEqual(n,                 type.tagIndex);
            AreEqual(SchemaTypeKind.Tag, type.kind);
            IsNull  (type.componentKey);
        }
        AreEqual(3,                     schema.TagTypeByType.Count);
        AreEqual(3,                     schema.TagTypeByName.Count);
        {
            var testTagType = schema.TagTypeByType[typeof(TestTag)];
            AreEqual(typeof(TestTag),       testTagType.type);
            AreEqual("tag: [#TestTag]",     testTagType.ToString());
        } {
            var testTagType = schema.GetTagType<TestTag>();
            AreEqual(typeof(TestTag),       testTagType.type);
            AreEqual("tag: [#TestTag]",     testTagType.ToString());
        } {
            var testTagType = schema.TagTypeByName["test-tag"];
            AreEqual(typeof(TestTag),       testTagType.type);
            AreEqual("tag: [#TestTag]",     testTagType.ToString());
        } {
            var testTagType = schema.TagTypeByName[nameof(TestTag3)];
            AreEqual(typeof(TestTag3),      testTagType.type);
            AreEqual("tag: [#TestTag3]",    testTagType.ToString());
        }
    }
    
    [Test]
    public static void Test_ComponentTypes()
    {
        var schema      = EntityStore.GetEntitySchema();
        var components  = schema.Components;
        var scripts     = schema.Scripts;
        
        AreEqual("components: 16  scripts: 6  entity tags: 3", schema.ToString());
        AreEqual(17,    components.Length);
        AreEqual( 7,    scripts.Length);
        
        AreEqual(22,    schema.SchemaTypeByKey.Count);
        AreEqual(16,    schema.ComponentTypeByType.Count);
        AreEqual( 6,    schema.ScriptTypeByType.Count);
        
        IsNull(components[0]);
        for (int n = 1; n < components.Length; n++) {
            var type = components[n];
            AreEqual(n, type.structIndex);
            AreEqual(SchemaTypeKind.Component, type.kind);
            NotNull (type.componentKey);
        }
        {
            var schemaType = schema.SchemaTypeByKey["pos"];
            AreEqual(typeof(Position), schemaType.type);
        } {
            var schemaType = schema.SchemaTypeByKey["test"];
            AreEqual(typeof(TestComponent), schemaType.type);
        } {
            var componentType = schema.GetComponentType<MyComponent1>();
            AreEqual("my1",                                 componentType.componentKey);
            AreEqual("component: 'my1' [MyComponent1]",     componentType.ToString());
        }
        // --- Engine.ECS types
        AssertBlittableComponent<Position>      (schema, true);
        AssertBlittableComponent<Rotation>      (schema, true);
        AssertBlittableComponent<Scale3>        (schema, true);
        AssertBlittableComponent<Transform>     (schema, true);
        AssertBlittableComponent<EntityName>    (schema, true);
        AssertBlittableComponent<Unresolved>    (schema, false);
        
        // --- BCL types
        AssertBlittableComponent<BlittableDatetime>     (schema, true);
        AssertBlittableComponent<BlittableGuid>         (schema, true);
        AssertBlittableComponent<BlittableBigInteger>   (schema, true);
        
        
        // --- Test blittable types
        AssertBlittableComponent<MyComponent1>  (schema, true);
        AssertBlittableComponent<MyComponent1>  (schema, true);
        AssertBlittableComponent<ByteComponent> (schema, true);
        
        // --- Test non-blittable types
        AssertBlittableComponent<NonBlittableArray>     (schema, false);
        AssertBlittableComponent<NonBlittableList>      (schema, false);
        AssertBlittableComponent<NonBlittableDictionary>(schema, false);
    }
    
    private static void AssertBlittableComponent<T>(EntitySchema schema, bool expect) where T : struct, IComponent {
        var componentType = schema.ComponentTypeByType[typeof(T)];
        AreEqual(expect, componentType.blittable);
    }
    
    private static void AssertBlittableScript<T>(EntitySchema schema, bool expect)  where T : Script {
        var scriptType = schema.ScriptTypeByType[typeof(T)];
        AreEqual(expect, scriptType.blittable);
    }
    
    [Test]
    public static void Test_ScriptTypes()
    {
        var schema      = EntityStore.GetEntitySchema();
        var scripts     = schema.Scripts;
        IsNull(scripts[0]);
        for (int n = 1; n < scripts.Length; n++) {
            var type = scripts[n];
            AreEqual(n, type.scriptIndex);
            AreEqual(SchemaTypeKind.Script, type.kind);
            NotNull (type.componentKey);
        }
        
        var scriptType = schema.GetScriptType<TestComponent>();
        AreEqual("test",                                scriptType.componentKey);
        AreEqual("script: 'test' [*TestComponent]",     scriptType.ToString());
        
        AreEqual(typeof(Position),  schema.SchemaTypeByKey["pos"].type);
        AreEqual("test",            schema.ScriptTypeByType[typeof(TestComponent)].componentKey);
        
        AssertBlittableScript<TestComponent>(schema, true);
    }

}
