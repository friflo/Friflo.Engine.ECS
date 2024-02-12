using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_Entity
{
    [Test]
    public static void Test_Entity_Components()
    {
        var store = new EntityStore();
        var entity = store.CreateEntity();
        AreEqual("IComponent[0]", entity.Components.ToString());
        
        entity.AddComponent(new Position(1, 2, 3));
        entity.AddComponent(new EntityName("test"));
       
        AreEqual("IComponent[2]", entity.Components.ToString());
        var components = entity.Components.Array;
        AreEqual(2, components.Length);
        AreEqual("test",                ((EntityName)components[0]).value);
        AreEqual(new Position(1,2,3),   (Position)components[1]);
    }
}

