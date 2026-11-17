using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Serialize {

public struct TenfoldValue : IComponent { public int Value; }

/// <summary> Writes <see cref="TenfoldValue"/> as the object <c>{"Tenfold": Value * 10}</c> instead of <c>{"Value": Value}</c>, to show that the mapper is used. </summary>
public sealed class TenfoldValueMapper : TypeMapper<TenfoldValue>
{
    private TypeMapper<Dictionary<string, int>> dictionaryMapper;
    
    public override void InitTypeMapper(TypeStore typeStore) {
        dictionaryMapper = (TypeMapper<Dictionary<string, int>>)typeStore.GetTypeMapper(typeof(Dictionary<string, int>));
    }
    
    public override bool IsNull(ref TenfoldValue value) => false;
    
    public override void Write(ref Writer writer, TenfoldValue slot) {
        dictionaryMapper.Write(ref writer, new Dictionary<string, int> { ["Tenfold"] = slot.Value * 10 });
    }
    
    public override TenfoldValue Read(ref Reader reader, TenfoldValue slot, out bool success) {
        var entries = dictionaryMapper.Read(ref reader, new Dictionary<string, int>(), out success);
        return new TenfoldValue { Value = success && entries != null && entries.TryGetValue("Tenfold", out var number) ? number / 10 : 0 };
    }
}

public sealed class TenfoldValueMatcher : ITypeMatcher
{
    public TypeMapper MatchTypeMapper(Type type, StoreConfig config) => type == typeof(TenfoldValue) ? new TenfoldValueMapper() : null;
}

public static class Test_TypeMatcher
{
    private static string Write(EntityStore store) {
        var stream = new MemoryStream();
        new EntitySerializer().WriteStore(store, stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
    
    /// <summary>
    /// The mapper of a type is chosen when the type is used the first time and the type store is shared by the process,
    /// so a matcher can only be added before anything was serialized. This test runs only if it is the first one in the process
    /// (run it alone with a filter) and is ignored otherwise.
    /// </summary>
    [Test]
    public static void Test_TypeMatcher_is_used_for_a_component_type()
    {
        try {
            EntitySerializer.AddTypeMatcher(new TenfoldValueMatcher());
        } catch (InvalidOperationException) {
            Ignore("A mapper was already chosen in this process. Run this test alone.");
        }
        var store = new EntityStore();
        store.CreateEntity(new TenfoldValue { Value = 7 });
        
        var json = Write(store);
        StringAssert.Contains("\"TenfoldValue\":{\"Tenfold\":70}", json.Replace(" ", "").Replace("\n", "").Replace("\r", ""));
        
        var copy = new EntityStore();
        var result = new EntitySerializer().ReadIntoStore(copy, new MemoryStream(Encoding.UTF8.GetBytes(json)));
        IsNull(result.error);
        AreEqual(7, copy.GetEntityById(1).GetComponent<TenfoldValue>().Value);
    }
    
    [Test]
    public static void Test_TypeMatcher_refused_after_the_first_serialization()
    {
        var store = new EntityStore();
        store.CreateEntity(new Position { x = 1 });
        Write(store);
        
        var e1 = Throws<InvalidOperationException>(() => EntitySerializer.AddTypeMatcher(new TenfoldValueMatcher()));
        StringAssert.Contains("before the first entity is serialized", e1!.Message);
        var e2 = Throws<InvalidOperationException>(() => EntitySerializer.AddTypeMapper(new TenfoldValueMapper()));
        StringAssert.Contains("before the first entity is serialized", e2!.Message);
    }
    
    [Test]
    public static void Test_TypeMatcher_null_arguments()
    {
        Throws<ArgumentNullException>(() => EntitySerializer.AddTypeMatcher(null));
        Throws<ArgumentNullException>(() => EntitySerializer.AddTypeMapper(null));
    }
}

}
