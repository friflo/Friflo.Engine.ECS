using System;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_ComponentTypes
{
    [Test]
    public static void Test_ComponentTypes_basics()
    {
        var twoStructs = ComponentTypes.Get<Position, Rotation>();
        AreEqual("Components: [Position, Rotation]",  twoStructs.ToString());
        
        var types    = new ComponentTypes();
        AreEqual("Components: []",                    types.ToString());
        IsFalse(types.Has<Position>());
        IsFalse(types.HasAll(twoStructs));
        IsFalse(types.HasAny(twoStructs));
        
        types.Add<Position>();
        IsTrue (types.Has<Position>());
        IsFalse(types.HasAll(twoStructs));
        IsTrue (types.HasAny(twoStructs));
        
        AreEqual("Components: [Position]",            types.ToString());
        
        types.Add<Rotation>();
        AreEqual("Components: [Position, Rotation]",  types.ToString());
        IsFalse(types.Has<MyComponent2, Rotation>());
        IsTrue (types.Has<Position, Rotation>());
        
        IsFalse(types.Has<MyComponent2, Rotation, Scale3>());
        IsFalse(types.Has<MyComponent2, MyComponent3, Scale3>());
        IsFalse(types.Has<Position, Rotation, Scale3>());
        IsTrue (types.HasAll(twoStructs));
        IsTrue (types.HasAny(twoStructs));

        var copy = new ComponentTypes();
        copy.Add(types);
        AreEqual("Components: [Position, Rotation]",  copy.ToString());
        
        copy.Remove<Position>();
        AreEqual("Components: [Rotation]",            copy.ToString());
        
        copy.Remove(types);
        AreEqual("Components: []",                    copy.ToString());
    }
    
    [Test]
    public static void Test_ComponentTypes_constructor()
    {
        var schema          = EntityStore.GetEntitySchema();
        var positionType    = schema.GetComponentType<Position>();
        
        var types = new ComponentTypes(positionType);
        AreEqual(1, types.Count);
        IsTrue  (types.Has<Position>());
    }
    
    [Test]
    public static void Test_ComponentTypes_Get()
    {
        var schema = EntityStore.GetEntitySchema();
#if !UNITY_5_3_OR_NEWER
        AreEqual(4, schema.EngineDependants.Length);
#endif
        var engine = schema.EngineDependants[0];
        AreEqual("Friflo.Engine.ECS",   engine.AssemblyName);
        AreEqual("Friflo.Engine.ECS",   engine.ToString());
        AreEqual(9,                     engine.Types.Length);
        foreach (var type in engine.Types) {
            AreSame(engine.Assembly, type.Type.Assembly);
        }
        
        var testStructType  = schema.ComponentTypeByType[typeof(Position)];
        
        var struct1    = ComponentTypes.Get<Position>();
        AreEqual("Components: [Position]", struct1.ToString());
        int count1 = 0;
        foreach (var structType in struct1) {
            AreSame(testStructType, structType);
            count1++;
        }
        AreEqual(1, count1);
        
        var count2 = 0;
        var struct2 = ComponentTypes.Get<Position, Rotation>();
        AreEqual("Components: [Position, Rotation]", struct2.ToString());
        foreach (var _ in struct2) {
            count2++;
        }
        AreEqual(2, count2);
        
        AreEqual(struct2, ComponentTypes.Get<Position, Rotation>());
    }
    
    [Test]
    public static void Test_ComponentTypes_Get_Mem()
    {
        var struct1    = ComponentTypes.Get<Position>();
        foreach (var _ in struct1) { }
        
        // --- 1 struct
        var start   = Mem.GetAllocatedBytes();
        int count1 = 0;
        foreach (var _ in struct1) {
            count1++;
        }
        Mem.AssertNoAlloc(start);
        AreEqual(1, count1);
        
        // --- 2 components
        start       = Mem.GetAllocatedBytes();
        var struct2 = ComponentTypes.Get<Position, Rotation>();
        var count2 = 0;
        foreach (var _ in struct2) {
            count2++;
        }
        Mem.AssertNoAlloc(start);
        AreEqual(2, count2);
    }
    
    [Test]
    public static void Test_ComponentTypes_Enumerator_Reset()
    {
        var types       = ComponentTypes.Get<Position>();
        var enumerator  = types.GetEnumerator();
        while (enumerator.MoveNext()) { }
        enumerator.Reset();
        int count = 0;
        while (enumerator.MoveNext()) {
            count++;
        }
        AreEqual(1, count);
        enumerator.Dispose();
    }
    
    [Test]
    public static void Test_ComponentTypes_generic_IEnumerator()
    {
        IEnumerable<ComponentType> tags = ComponentTypes.Get<Position>();
        int count = 0;
        foreach (var _ in tags) {
            count++;
        }
        AreEqual(1, count);
    }
    
    [Test]
    public static void Test_ComponentTypes_Tags()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var type        = store.GetArchetype(default, Tags.Get<TestTag2, TestTag3>());
        var tags        = type.Tags;
        AreEqual(2, tags.Count);
        var enumerator =  tags.GetEnumerator();
        IsTrue(enumerator.MoveNext());
        AreEqual(typeof(TestTag2), enumerator.Current!.Type);
        
        IsTrue(enumerator.MoveNext());
        AreEqual(typeof(TestTag3), enumerator.Current!.Type);
        
        IsFalse(enumerator.MoveNext());
        enumerator.Dispose();
    }
    
    [Test]
    public static void Test_ComponentTypes_lookup_structs_and_tags_Perf()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var type1   = store.GetArchetype(ComponentTypes.Get<Position>());
        var result  = store.FindArchetype(type1.ComponentTypes, type1.Tags);
        AreEqual(1, type1.ComponentTypes.Count);
        AreSame (type1, result);
        
        result = store.FindArchetype(ComponentTypes.Get<UniqueEntity, Scale3>(), Tags.Get<TestTag2, TestTag3, TestTag5>());
        IsNull(result);
        
        var start   = Mem.GetAllocatedBytes();
        var types   = type1.ComponentTypes;
        var tags    = type1.Tags;
        store.FindArchetype(types, tags);
        Mem.AssertNoAlloc(start);
        
        var sw = new Stopwatch();
        var count   = 10; // 100_000_000 ~ #PC: 1.163 ms
        sw.Start();
        for (int n = 0; n < count; n++)
        {
            store.FindArchetype(types, tags);
        }
        Console.WriteLine($"FindArchetype - duration: {sw.ElapsedMilliseconds} ms");
    }
}

}