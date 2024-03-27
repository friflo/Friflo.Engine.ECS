using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {

public static class Test_ComponentSchema
{
    [Test]
    public static void Test_EntityTags() {
        var schema      = EntityStore.GetEntitySchema();
        AreEqual(11,     schema.Tags.Length);
        
        var tags = schema.Tags;
        IsNull(tags[0]);
        for (int n = 1; n < tags.Length; n++) {
            var type = tags[n];
            AreEqual(n,                 type.TagIndex);
            AreEqual(SchemaTypeKind.Tag, type.Kind);
            IsNull  (type.ComponentKey);
        }
        AreEqual(10,                     schema.TagTypeByType.Count);
        AreEqual(10,                     schema.TagTypeByName.Count);
        {
            var testTagType = schema.TagTypeByType[typeof(TestTag)];
            AreEqual(typeof(TestTag),       testTagType.Type);
            AreEqual("tag: [#TestTag]",     testTagType.ToString());
        } {
            var testTagType = schema.GetTagType<TestTag>();
            AreEqual(typeof(TestTag),       testTagType.Type);
            AreEqual("tag: [#TestTag]",     testTagType.ToString());
        } {
            var testTagType = schema.TagTypeByName["test-tag"];
            AreEqual(typeof(TestTag),       testTagType.Type);
            AreEqual("tag: [#TestTag]",     testTagType.ToString());
        } {
            var testTagType = schema.TagTypeByName[nameof(TestTag3)];
            AreEqual(typeof(TestTag3),      testTagType.Type);
            AreEqual("tag: [#TestTag3]",    testTagType.ToString());
        }
    }
    
    [Test]
    public static void Test_ComponentTypes()
    {
        var schema      = EntityStore.GetEntitySchema();
        var components  = schema.Components;
        var scripts     = schema.Scripts;
        
        AreEqual("components: 33  scripts: 8  entity tags: 10", schema.ToString());
        AreEqual(34,    components.Length);
        AreEqual( 9,    scripts.Length);
        
        AreEqual(41,    schema.SchemaTypeByKey.Count);
        AreEqual(33,    schema.ComponentTypeByType.Count);
        AreEqual( 8,    schema.ScriptTypeByType.Count);
        
        IsNull(components[0]);
        for (int n = 1; n < components.Length; n++) {
            var type = components[n];
            AreEqual(n, type.StructIndex);
            AreEqual(SchemaTypeKind.Component, type.Kind);
            NotNull (type.ComponentKey);
        }
        {
            var schemaType = schema.SchemaTypeByKey["pos"];
            AreEqual(typeof(Position), schemaType.Type);
        } {
            var schemaType = schema.SchemaTypeByKey["test"];
            AreEqual(typeof(TestComponent), schemaType.Type);
        } {
            var componentType = schema.GetComponentType<MyComponent1>();
            AreEqual("my1",                             componentType.ComponentKey);
            AreEqual("Component: [MyComponent1]",       componentType.ToString());
            AreEqual(4,                                 componentType.StructSize);
            IsTrue  (                                   componentType.IsBlittable);
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
        AreEqual(expect, componentType.IsBlittable);
    }
    
    private static void AssertBlittableScript<T>(EntitySchema schema, bool expect)  where T : Script {
        var scriptType = schema.ScriptTypeByType[typeof(T)];
        AreEqual(expect, scriptType.IsBlittable);
    }
    
    [Test]
    public static void Test_ScriptTypes()
    {
        var schema      = EntityStore.GetEntitySchema();
        var scripts     = schema.Scripts;
        IsNull(scripts[0]);
        for (int n = 1; n < scripts.Length; n++) {
            var type = scripts[n];
            AreEqual(n, type.ScriptIndex);
            AreEqual(SchemaTypeKind.Script, type.Kind);
            NotNull (type.ComponentKey);
        }
        
        var scriptType = schema.GetScriptType<TestComponent>();
        AreEqual("test",                        scriptType.ComponentKey);
        AreEqual("Script: [*TestComponent]",    scriptType.ToString());
        
        AreEqual(typeof(Position),  schema.SchemaTypeByKey["pos"].Type);
        AreEqual("test",            schema.ScriptTypeByType[typeof(TestComponent)].ComponentKey);
        
        AssertBlittableScript<TestComponent>(schema, true);
    }

}

}