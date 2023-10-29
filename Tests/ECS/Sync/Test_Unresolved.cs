using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Sync;

public static class Test_Unresolved
{
    private static JsonValue UnresolvedComponents =>
        new JsonValue("{ \"xxx\": { \"foo\":1 }}");
    
    [Test]
    public static void Test_Unresolved_read_components()
    {
        var store       = new GameEntityStore(PidType.UsePidAsId);
        var converter   = EntityConverter.Default;
        
        var dataEntity  = new DataEntity { pid = 1, components = UnresolvedComponents };
        
        for (int n = 0; n < 2; n++)
        {
            var gameEntity  = converter.DataToGameEntity(dataEntity, store, out _);
            var unresolved  = gameEntity.GetComponent<Unresolved>();
            
            AreEqual(1,                 unresolved.components.Count);
            AreEqual("{ \"foo\":1 }",   unresolved.components["xxx"].ToString());
            
            AreEqual("unresolved components: 'xxx'", unresolved.ToString());
        }
    }
}

