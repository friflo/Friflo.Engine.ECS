using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;


public static class Test_Entity
{
    
    
    [Test]
    public static void Test_Entity_non_generic_Script_methods()
    {
        var store       = new EntityStore();
        var entity      = store.CreateEntity();
        var schema      = EntityStore.GetEntitySchema();
        var scriptType  = schema.ScriptTypeByType[typeof(TestScript1)];
        
        Entity.AddEntityScript(entity, scriptType);
        var script = Entity.GetEntityScript(entity, scriptType);
        AreEqual(1,                     entity.Scripts.Length);
        AreSame(typeof(TestScript1),    script.GetType());
        
        Entity.RemoveEntityScript(entity, scriptType);
        AreEqual(0,                     entity.Scripts.Length);
        
        // remove same script type again
        Entity.RemoveEntityScript(entity, scriptType);
        AreEqual(0,                     entity.Scripts.Length);
    }
    
    [Test]
    public static void Test_Entity_non_generic_Component_methods()
    {
        var store           = new EntityStore();
        var entity          = store.CreateEntity();
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(EntityName)];
        
        Entity.AddEntityComponent(entity, componentType);
        var component = Entity.GetEntityComponent(entity, componentType);
        AreEqual(1,                     entity.Archetype.ComponentCount);
        AreSame(typeof(EntityName),     component.GetType());
        
        Entity.RemoveEntityComponent(entity, componentType);
        AreEqual(0,                     entity.Archetype.ComponentCount);
    }
}




