using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS {

internal struct AttackComponent : IIndexedComponent<Entity> {
    public      Entity  GetIndexedValue() => target;
    internal    Entity  target;
}


internal struct IndexedName : IIndexedComponent<string> {
    public      string  GetIndexedValue() => name;
    internal    string  name;

    public override string ToString() => name;
}

internal struct IndexedInt : IIndexedComponent<int> {
    public      int     GetIndexedValue() => value;
    internal    int     value;
    
    public override string ToString() => value.ToString();
}


public static class Test_Lab
{
    [Test]
    public static void Test_Lab_EntityLink()
    {
        var world = new EntityStore();
        var targets = new EntityList(world);
        for (int n = 0; n < 10; n++) {
            targets.Add(world.CreateEntity());
        }
        var entities = new List<Entity>();
        for (int n = 0; n < 10; n++) {
            var entity = world.CreateEntity(new Position(n, 0, 0), new AttackComponent{target = targets[n]});
            entities.Add(entity);
        }
        Entity target = targets[0];
        
        entities[0].AddComponent(new IndexedName   { name   = "find-me" });
        entities[1].AddComponent(new IndexedInt    { value  = 123       });
        entities[2].AddComponent(new IndexedName   { name   = "find-me" });
        entities[2].AddComponent(new IndexedInt    { value  = 123       });
    //  entities[1].AddComponent(new AttackComponent { target = target }); // todo throws NotImplementedException : to avoid excessive boxing. ...
        
        var query1  = world.Query<Position, IndexedName>().     Has<IndexedName,   string>("find-me");
        var query2  = world.Query<Position, IndexedInt>().      Has<IndexedInt,    int>   (123);
        var query3  = world.Query<IndexedName, IndexedInt>().   Has<IndexedName,   string>("find-me").
                                                                Has<IndexedInt,    int>   (123);
        var query4  = world.Query().                            Has<IndexedName,   string>("find-me").
                                                                Has<IndexedInt,    int>   (123);
        
        var query5  = world.Query<Position, AttackComponent>().Has<AttackComponent, Entity>(target);
        {
            int count = 0;
            query1.ForEachEntity((ref Position _, ref IndexedName indexedName, Entity _) => {
                count++;
                AreEqual("find-me", indexedName.name);
            });
            AreEqual(2, count);
        } { 
            int count = 0;
            query2.ForEachEntity((ref Position _, ref IndexedInt indexedInt, Entity _) => {
                count++;
                AreEqual(123, indexedInt.value);
            });
            AreEqual(2, count);
        } { 
            var count = 0;
            query3.ForEachEntity((ref IndexedName _, ref IndexedInt _, Entity _) => {
                count++;
            });
            AreEqual(1, count);
        }
        AreEqual(3, query4.Entities.Count);
        
        query5.ForEachEntity((ref Position _, ref AttackComponent attack, Entity _) => {
            AreEqual(target, attack.target);
        });
    }
    
    [Test]
    public static void Test_AvoidBoxing()
    {
        var indexedName = new IndexedInt { value = 123 };
        
        var getter = CreateGetValue<IndexedInt,int>();
        var value = getter(indexedName);
        AreEqual(123, value);
        
        var start = GC.GetAllocatedBytesForCurrentThread();
        getter(indexedName);
        var diff = GC.GetAllocatedBytesForCurrentThread() - start;
        Console.WriteLine($"diff: {diff}");
    }
    
    // https://stackoverflow.com/questions/61185678/c-sharp-casting-t-where-t-struct-to-an-interface-without-boxing
    private static GetIndexedValue<T,V> CreateGetValue<T,V>() where T : struct, IComponent
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(Test_Lab).GetMethod(nameof(GetIndexedComponentValue), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(T), typeof(V));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetIndexedValue<T,V>), genericMethod);
        return (GetIndexedValue<T,V>)genericDelegate;
    }
    
    internal static V GetIndexedComponentValue<T,V>(in T component) where T : struct, IIndexedComponent<V> {
        return component.GetIndexedValue();
    }
    
    internal delegate V GetIndexedValue<T, out V>(in T component) where T : struct, IComponent;
}

}
