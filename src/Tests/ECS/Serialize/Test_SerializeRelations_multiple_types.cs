using System.IO;
using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;
using Tests.ECS.Relations;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

/// <summary> An entity with several relation types: every type is written and every type is read back. </summary>
public static class Test_SerializeRelations_multiple_types
{
    private static EntityStore RoundTrip(EntityStore store)
    {
        var serializer = new EntitySerializer();
        using var writeStream = new MemoryStream();
        serializer.WriteStore(store, writeStream);
        
        writeStream.Position = 0;
        var copy    = new EntityStore();
        var result  = new EntitySerializer().ReadIntoStore(copy, writeStream);
        IsNull(result.error);
        return copy;
    }
    
    private static EntityStore ReadJson(string json)
    {
        var store   = new EntityStore();
        var result  = new EntitySerializer().ReadIntoStore(store, new MemoryStream(Encoding.UTF8.GetBytes(json)));
        IsNull(result.error);
        return store;
    }
    
    [Test]
    public static void Test_SerializeRelations_two_relation_types_roundtrip()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity(1);
        entity.AddRelation(new IntRelation      { value = 101 });
        entity.AddRelation(new IntRelation      { value = 102 });
        entity.AddRelation(new StringRelation   { value = "a" });
        entity.AddComponent(new EntityName("test"));
        
        var back = RoundTrip(store).GetEntityById(1);
        AreEqual(2,     back.GetRelations<IntRelation>().Length);
        AreEqual(1,     back.GetRelations<StringRelation>().Length);
        AreEqual("a",   back.GetRelations<StringRelation>()[0].value);
        AreEqual("test",back.GetComponent<EntityName>().value);
    }
    
    [Test]
    public static void Test_SerializeRelations_link_relation_and_two_more_types_roundtrip()
    {
        var store   = new EntityStore();
        var target1 = store.CreateEntity(2);
        var target2 = store.CreateEntity(3);
        var entity  = store.CreateEntity(1);
        entity.AddRelation(new AttackRelation   { speed = 5, target = target1 });
        entity.AddRelation(new AttackRelation   { speed = 6, target = target2 });
        entity.AddRelation(new IntRelation      { value = 7 });
        entity.AddRelation(new StringRelation   { value = "x" });
        entity.AddRelation(new StringRelation   { value = "y" });
        
        var back = RoundTrip(store).GetEntityById(1);
        AreEqual(2, back.GetRelations<AttackRelation>().Length);
        AreEqual(1, back.GetRelations<IntRelation>().Length);
        AreEqual(2, back.GetRelations<StringRelation>().Length);
    }
    
    [Test]
    public static void Test_SerializeRelations_single_relation_type_with_several_targets_roundtrip()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity(1);
        entity.AddRelation(new IntRelation { value = 1 });
        entity.AddRelation(new IntRelation { value = 2 });
        entity.AddRelation(new IntRelation { value = 3 });
        
        AreEqual(3, RoundTrip(store).GetEntityById(1).GetRelations<IntRelation>().Length);
    }
    
    [Test]
    public static void Test_SerializeRelations_component_after_relations_is_read()
    {
        const string json = @"[{""id"":1,""components"":{""IntRelation"":[{""value"":1}],""StringRelation"":[{""value"":""s""}],""name"":{""value"":""late""}}}]";
        var back = ReadJson(json).GetEntityById(1);
        AreEqual(1,         back.GetRelations<IntRelation>().Length);
        AreEqual(1,         back.GetRelations<StringRelation>().Length);
        AreEqual("late",    back.GetComponent<EntityName>().value);
    }
    
    [Test]
    public static void Test_SerializeRelations_empty_relation_array_followed_by_another_type()
    {
        const string json = @"[{""id"":1,""components"":{""IntRelation"":[],""StringRelation"":[{""value"":""s""}]}}]";
        var back = ReadJson(json).GetEntityById(1);
        AreEqual(0, back.GetRelations<IntRelation>().Length);
        AreEqual(1, back.GetRelations<StringRelation>().Length);
    }
}

}
