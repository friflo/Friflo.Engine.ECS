using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_SchemaType
{
    /// <summary>
    /// Ensure initialization of <see cref="ScriptInfo{T}.Index"/>.
    /// Especially if <see cref="Tags.Get{T}"/> is the first call in an application.  
    /// </summary>
    [Test]
    public static void Test_SchemaType_Script_Index()
    {
        var scriptIndex = ScriptInfo<TestScript1>.Index;
        var schema      = EntityStore.GetEntitySchema();
        var scriptType  = schema.scripts[scriptIndex];
        
        AreEqual("TestScript1",         scriptType.Name);
        AreEqual(scriptIndex,           scriptType.ScriptIndex);
        AreEqual(typeof(TestScript1),   scriptType.Type);
    }

    /// <summary>
    /// Ensure initialization of <see cref="TagInfo{T}.Index"/>.
    /// </summary>
    [Test]
    public static void Test_SchemaType_Tag_Index()
    {
        var tagIndex    = TagInfo<TestTag>.Index;
        var schema      = EntityStore.GetEntitySchema();
        var tagType     = schema.tags[tagIndex];
        
        AreEqual("TestTag",         tagType.Name);
        AreEqual(tagIndex,          tagType.TagIndex);
        AreEqual(typeof(TestTag),   tagType.Type);
    }
    
    /// <summary>
    /// Ensure initialization of <see cref="StructInfo{T}.Index"/>.
    /// </summary>
    [Test]
    public static void Test_SchemaType_StructIndex()
    {
        int count = 0;
        var componentTypes = ComponentTypes.Get<Position>();
        foreach (var type in componentTypes) {
            count++;
            AreEqual("Position",        type.Name);
            AreEqual(typeof(Position),  type.Type);
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_SchemaType_SchemaTypes_AddSchemaType()
    {
        var schemaTypes = new SchemaTypes();
        var assemblyType = new AssemblyType(null, (SchemaTypeKind)255, 0);
        schemaTypes.AddSchemaType(assemblyType);
    }
        
    [Test]
    public static void Test_SchemaType_Tags_Get_Perf()
    {
        var count   = 10; // 10_000_000_000 ~ #PC: 2499 ms
        var sw = new Stopwatch();
        sw.Start();
        for (long n = 0; n < count; n++) {
            Tags.Get<TestTag>();
        }
        Console.WriteLine($"Tags.Get<>() - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_SchemaType_ComponentTypes_Get_Perf()
    {
        var count   = 10; // 10_000_000_000 ~ #PC: 2544 ms
        var sw = new Stopwatch();
        sw.Start();
        for (long n = 0; n < count; n++) {
            ComponentTypes.Get<MyComponent1>();
        }
        Console.WriteLine($"ComponentTypes.Get<>() - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    // -------------------------- access Entity components performance --------------------------
    [Test]
    public static void Test_SchemaType_Entity_HasComponent_Perf()
    {
        var count   = 10; // 1_000_000_000 ~ #PC: 522 ms
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position());
        var sw      = new Stopwatch();
        sw.Start();
        for (long n = 0; n < count; n++) {
            entity.HasComponent<Position>();
        }
        Console.WriteLine($"Entity.HasComponent<>() - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_SchemaType_Entity_GetComponent_Perf()
    {
        var count   = 10; // 1_000_000_000 ~ #PC: 3055 ms
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position());
        var sw      = new Stopwatch();
        sw.Start();
        for (long n = 0; n < count; n++) {
            entity.GetComponent<Position>();
        }
        Console.WriteLine($"Entity.GetComponent<>() - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public static void Test_SchemaType_Entity_Position_Perf()
    {
        var count   = 10; // 1_000_000_000 ~ #PC: 983 ms
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new Position());
        var sw      = new Stopwatch();
        sw.Start();
        for (long n = 0; n < count; n++) {
            _ = entity.Position;
        }
        Console.WriteLine($"Entity.Position - duration: {sw.ElapsedMilliseconds} ms");
    }
    
    /// <summary> performance reference for <see cref="Test_SchemaType_Entity_Position_Perf"/> </summary>
    [Test]
    public static void Test_SchemaType_Field_access_Perf()
    {
        var player  = new PlayerRef { position = default };
        long count  = 10; // 1_000_000_000L ~ #PC: 381 ms
        var sw      = new Stopwatch();
        sw.Start();
        for (var n = 0; n < count; n++) {
            _ = player.position;
        }
        Console.WriteLine($"Field_access - duration: {sw.ElapsedMilliseconds} ms");
    }

    private class PlayerRef {
        public Position position;
    }
}

}
