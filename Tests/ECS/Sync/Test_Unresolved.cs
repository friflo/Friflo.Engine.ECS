using System.Collections.Generic;
using System.Linq;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Sync;

public static class Test_Unresolved
{
    [Test]
    public static void Test_Unresolved_components()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var converter       = EntityConverter.Default;
        var source1         = new DataEntity { pid = 1, components = new JsonValue("{ \"xxx1\": { \"foo1\":1 }}") };
        var unresolvedQuery = store.Query(Signature.Get<Unresolved>());
        
        for (int n = 0; n < 2; n++)
        {
            var gameEntity  = converter.DataToGameEntity(source1, store, out _);
            var unresolved  = gameEntity.GetComponent<Unresolved>();
            
            AreEqual(1,                                         unresolved.components.Length);
            AreEqual("\"xxx1\": { \"foo1\":1 }",                unresolved.components[0].ToString());
            AreEqual("unresolved components: \"xxx1\"",         unresolved.ToString());
            AreEqual(1,                                         unresolvedQuery.EntityCount);
            
            var targetEntity = converter.GameToDataEntity(gameEntity);
            AreEqual("{\"xxx1\":{ \"foo1\":1 }}",               targetEntity.components.ToString());
        }
        
        var source2     = new DataEntity { pid = 1, components = new JsonValue("{ \"xxx2\": { \"foo2\":2 }}") };
        for (int n = 0; n < 2; n++)
        {
            var gameEntity  = converter.DataToGameEntity(source2, store, out _);
            var unresolved  = gameEntity.GetComponent<Unresolved>();
            AreEqual(2,                                             unresolved.components.Length);
            AreEqual("\"xxx1\": { \"foo1\":1 }",                    unresolved.components[0].ToString());
            AreEqual("\"xxx2\": { \"foo2\":2 }",                    unresolved.components[1].ToString());
            AreEqual("unresolved components: \"xxx1\", \"xxx2\"",   unresolved.ToString());
            AreEqual(1,                                             unresolvedQuery.EntityCount);
            
            var targetEntity = converter.GameToDataEntity(gameEntity);
            AreEqual("{\"xxx1\":{ \"foo1\":1 },\"xxx2\":{ \"foo2\":2 }}", targetEntity.components.ToString());
        }
    }
    
    [Test]
    public static void Test_Unresolved_tags()
    {
        var store           = new EntityStore(PidType.UsePidAsId);
        var converter       = EntityConverter.Default;
        var source1         = new DataEntity { pid = 1, tags = new List<string>{"yyy1"} };
        var unresolvedQuery = store.Query(Signature.Get<Unresolved>());
        
        for (int n = 0; n < 2; n++)
        {
            var gameEntity  = converter.DataToGameEntity(source1, store, out _);
            var unresolved  = gameEntity.GetComponent<Unresolved>();
            
            AreEqual(1,                                 unresolved.tags.Length);
            IsTrue  (unresolved.tags.Contains("yyy1"));
            AreEqual("unresolved tags: \"yyy1\"",       unresolved.ToString());
            AreEqual(1,                                 unresolvedQuery.EntityCount);
            
            var targetEntity = converter.GameToDataEntity(gameEntity);
            AreEqual(1, targetEntity.tags.Count);
            IsTrue  (targetEntity.tags.Contains("yyy1"));
            IsTrue  (targetEntity.components.IsNull()); // Unresolved component is not serialized.
        }
        
        var source2     = new DataEntity { pid = 1, tags = new List<string>{"yyy2"} };
        for (int n = 0; n < 2; n++)
        {
            var gameEntity  = converter.DataToGameEntity(source2, store, out _);
            var unresolved  = gameEntity.GetComponent<Unresolved>();
            
            AreEqual(2,                                     unresolved.tags.Length);
            IsTrue  (unresolved.tags.Contains("yyy1"));
            IsTrue  (unresolved.tags.Contains("yyy2"));
            AreEqual("unresolved tags: \"yyy1\", \"yyy2\"", unresolved.ToString());
            AreEqual(1,                                     unresolvedQuery.EntityCount);
            
            var targetEntity = converter.GameToDataEntity(gameEntity);
            AreEqual(2, targetEntity.tags.Count);
            IsTrue  (targetEntity.tags.Contains("yyy1"));
            IsTrue  (targetEntity.tags.Contains("yyy2"));
            IsTrue  (targetEntity.components.IsNull()); // Unresolved component is not serialized.
        }
    }
}

