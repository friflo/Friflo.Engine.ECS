using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable EqualExpressionComparison
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.SoA {

public static class Test_SoA
{
    [Test]
    public static void Test_SoA_Add_Remove_Components()
    {
        var store = new EntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        var entity3 = store.CreateEntity();
        
        var pos1 = new Pos3SoA { value = new Vector3(11, 12, 13) };
        entity1.AddComponent(pos1);
        var result1 = entity1.GetSoA<Pos3SoA>();
        AreEqual(result1.value, pos1.value);
        
        var pos2 = new Pos3SoA { value = new Vector3(21, 22, 23) };
        entity2.AddComponent(pos2);
        var result2 = entity2.GetSoA<Pos3SoA>();
        AreEqual(result2.value, pos2.value);
        
        var pos3 = new Pos3SoA { value = new Vector3(31, 32, 33) };
        entity3.AddComponent(pos3);
        var result3 = entity3.GetSoA<Pos3SoA>();
        AreEqual(result3.value, pos3.value);
        
        entity1.RemoveComponent<Pos3SoA>();
        
        result2 = entity2.GetSoA<Pos3SoA>();
        AreEqual(result2.value, pos2.value);
        
        result3 = entity3.GetSoA<Pos3SoA>();
        AreEqual(result3.value, pos3.value);
    }
}

}
