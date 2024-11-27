/*
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github {

// https://github.com/friflo/Friflo.Json.Fliox/issues/56    
public struct CompA : IComponent
{
    public float2 goalLS;
}

public struct float2
{
    public float3 xyz{get;set;} // Error CS0523 : Struct member 'float2.xyz' of type 'float3' causes a cycle in the struct layout
};

public struct float3
{
    public float2 xy{get;set;}  // Error CS0523 : Struct member 'float3.xy' of type 'float2' causes a cycle in the struct layout
};

public static class Test_GitHub_56
{
    [Test]
    public static void Test_Serialize_enum_component_2()
    {
        var store = new EntityStore();
        Entity entity = store.CreateEntity(1);
        entity.AddComponent(new CompA { goalLS = new float2 { xyz = new float3 { }} });
    }
}

}
*/