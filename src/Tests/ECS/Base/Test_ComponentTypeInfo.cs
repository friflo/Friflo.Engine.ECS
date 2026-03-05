using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {
    
/// <summary>
/// Example to show how to attach additional type information to a component type.
/// The access to this information is simply an array index lookup.
/// Runtime costs: O(1) without memory allocation. 
/// </summary>
public static class ComponentTypeInfo<T>
{
    private static readonly bool[] isAssignableFromLookup = ComponentTypeInfoUtils.CreateIsAssignableFromLookup(typeof(T));
    
    public static bool IsAssignableFrom(ComponentType from) => isAssignableFromLookup[from.StructIndex];
}

internal static class ComponentTypeInfoUtils
{
    internal static bool[] CreateIsAssignableFromLookup(Type type)
    {
        var componentTypes = EntityStore.GetEntitySchema().Components;
        var lookup = new bool[componentTypes.Length];
        for (int n = 1; n < componentTypes.Length; n++) {
            lookup[n] = type.IsAssignableFrom(componentTypes[n].Type);
        }
        return lookup;
    }
}



public static class Test_ComponentTypeInfo
{
    interface ITestableInterface { }
    
    struct TestInterfaceComponent : IComponent, ITestableInterface { }
    
    [Test]
    public static void Test_IsAssignableFrom()
    {
        var store   = new EntityStore();
        var entity    = store.CreateEntity();
        entity.AddComponent(new Position());
        entity.AddComponent(new TestInterfaceComponent());

        foreach (var componentType in entity.Archetype.ComponentTypes)
        {
            bool isAssignableFrom = ComponentTypeInfo<ITestableInterface>.IsAssignableFrom(componentType);
            AreEqual(typeof(ITestableInterface).IsAssignableFrom(componentType.Type), isAssignableFrom);
            
            if (componentType.Type == typeof(TestInterfaceComponent)) {
                IsTrue(isAssignableFrom);
            } else {
                IsFalse(isAssignableFrom);
            }
        }
    }

    
#region CLR type info performance
    // [Test]
    // execution: 197 ms (Release) 
    public static void Test_IsAssignableFrom_Perf_is()
    {
        var component = new TestInterfaceComponent();
        for (int n = 0; n < 1_000_000_000; n++) {
            _ = component is ITestableInterface;
        }        
    }
    
    // [Test]
    // execution: 3373 ms (Release) 
    public static void Test_IsAssignableFrom_Perf_IsAssignableFrom()
    {
        var componentType = typeof(TestInterfaceComponent);
        for (int n = 0; n < 1_000_000_000; n++) {
            _ = typeof(ITestableInterface).IsAssignableFrom(componentType);
        }        
    }
    #endregion

}

}
