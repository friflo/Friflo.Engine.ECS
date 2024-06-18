using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
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

internal struct IndexedEntity : IIndexedComponent<Entity> {
    public      Entity  GetIndexedValue() => entity;
    internal    Entity  entity;
    
    public override string ToString() => entity.ToString();
}


[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
[SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations")]
public static class Test_Index
{
    [Test]
    public static void Test_Index_Component_Add_Remove()
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
        var entity0 = entities[0];
        var entity1 = entities[1];
        var entity2 = entities[2];
        
        entity0.AddComponent(new IndexedName   { name   = "find-me" });
        entity1.AddComponent(new IndexedInt    { value  = 123       });
        entity2.AddComponent(new IndexedName   { name   = "find-me" });
        entity2.AddComponent(new IndexedInt    { value  = 123       });
    //  entities[1].AddComponent(new AttackComponent { target = target }); // todo throws NotImplementedException : to avoid excessive boxing. ...
        
        var query1  = world.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me");
        var query2  = world.Query<Position,    IndexedInt>().   HasValue<IndexedInt,    int>   (123);
        var query3  = world.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me").
                                                                HasValue<IndexedInt,    int>   (123);
        var query4  = world.Query().                            HasValue<IndexedName,   string>("find-me").
                                                                HasValue<IndexedInt,    int>   (123);
        
        var query5  = world.Query<Position, AttackComponent>().HasValue<AttackComponent, Entity>(target);
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
        query5.ForEachEntity((ref Position _, ref AttackComponent attack, Entity _) => {
            AreEqual(target, attack.target);
        });
        
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] { 11, 13 },      query1.Entities.ToIds());
        AreEqual(2, query2.Entities.Count);     AreEqual(new int[] { 12, 13 },      query2.Entities.ToIds());
        AreEqual(1, query3.Entities.Count);     AreEqual(new int[] { 13 },          query3.Entities.ToIds());
        AreEqual(3, query4.Entities.Count);     AreEqual(new int[] { 11, 13, 12 },  query4.Entities.ToIds());
        
        entity2.RemoveComponent<IndexedName>();
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 11 },          query1.Entities.ToIds());
        AreEqual(2, query2.Entities.Count);     AreEqual(new int[] { 12, 13 },      query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(3, query4.Entities.Count);     AreEqual(new int[] { 11, 12, 13 },  query4.Entities.ToIds());
        
        entity2.RemoveComponent<IndexedInt>();
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 11 },          query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 12 },          query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(2, query4.Entities.Count);     AreEqual(new int[] { 11, 12 },      query4.Entities.ToIds());
        
        entity1.RemoveComponent<IndexedInt>();
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 11 },          query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] { },             query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(1, query4.Entities.Count);     AreEqual(new int[] { 11 },          query4.Entities.ToIds());
        
        entity0.RemoveComponent<IndexedName>();
        AreEqual(0, query1.Entities.Count);     AreEqual(new int[] { },             query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] { },             query2.Entities.ToIds());
        AreEqual(0, query3.Entities.Count);     AreEqual(new int[] { },             query3.Entities.ToIds());
        AreEqual(0, query4.Entities.Count);     AreEqual(new int[] { },             query4.Entities.ToIds());
    }
    
    /// <summary>
    /// Cover <see cref="HasValueIndex{TValue}.Add{TComponent}"/>
    /// </summary>
    [Test]
    public static void Test_Index_Component_Update()
    {
        var world = new EntityStore();

        var entities = new List<Entity>();
        for (int n = 0; n < 10; n++) {
            var entity = world.CreateEntity(new Position(n, 0, 0));
            entities.Add(entity);
        }
        var entity1 = entities[0];
        var entity2 = entities[1];
        var entity3 = entities[2];
        
        entity1.AddComponent(new IndexedName   { name   = "find-me1" });
        entity2.AddComponent(new IndexedInt    { value  = 123       });
        entity3.AddComponent(new IndexedName   { name   = "find-me1" });
        entity3.AddComponent(new IndexedInt    { value  = 123       });

        var query1  = world.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me1");
        var query2  = world.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me1");
        var query3  = world.Query().                            HasValue<IndexedName,   string>("find-me1");
        
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] { 1, 3 },        query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 3    },        query2.Entities.ToIds());
        AreEqual(2, query3.Entities.Count);     AreEqual(new int[] { 1, 3 },        query3.Entities.ToIds());
        
        // Add same value of indexed component again
        entity1.AddComponent(new IndexedName   { name   = "find-me1" });
        AreEqual(2, query1.Entities.Count);     AreEqual(new int[] { 1, 3 },    query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 3    },    query2.Entities.ToIds());
        AreEqual(2, query3.Entities.Count);     AreEqual(new int[] { 1, 3 },    query3.Entities.ToIds());
        
        // Update value of indexed component
        entity1.AddComponent(new IndexedName   { name   = "find-me2" });
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 3 },       query1.Entities.ToIds());
        AreEqual(1, query2.Entities.Count);     AreEqual(new int[] { 3 },       query2.Entities.ToIds());
        AreEqual(1, query3.Entities.Count);     AreEqual(new int[] { 3 },       query3.Entities.ToIds());

        // --- change queries
        query1  = world.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me2");
        query2  = world.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me2");
        query3  = world.Query().                            HasValue<IndexedName,   string>("find-me2");
        
        AreEqual(1, query1.Entities.Count);     AreEqual(new int[] { 1 },       query1.Entities.ToIds());
        AreEqual(0, query2.Entities.Count);     AreEqual(new int[] {   },       query2.Entities.ToIds());
        AreEqual(1, query3.Entities.Count);     AreEqual(new int[] { 1 },       query3.Entities.ToIds());
    }
    
    [Test]
    public static void Test_Index_indexed_Entity()
    {
        var world   = new EntityStore();
        var entity1 = world.CreateEntity(1);
        var entity2 = world.CreateEntity(2);
        var entity3 = world.CreateEntity(3);
        
        var target4 = world.CreateEntity(4);
        var target5 = world.CreateEntity(5);
        var target6 = world.CreateEntity(6);
        
        entity1.AddComponent(new IndexedEntity { entity = target4 });
        entity2.AddComponent(new IndexedEntity { entity = target5 });
        entity3.AddComponent(new IndexedEntity { entity = target5 });
        
        var query1  = world.Query().HasValue<IndexedEntity,   Entity>(target4);
        var query2  = world.Query().HasValue<IndexedEntity,   Entity>(target4).
                                    HasValue<IndexedEntity,   Entity>(target5);
        
        AreEqual(new int [] { 1       },    query1.Entities.ToIds());
        AreEqual(new int [] { 1, 2, 3 },    query2.Entities.ToIds());
        
        var references4 = target4.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 1       },    references4.Ids.ToArray());
        
        var references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 2, 3    },   references5.Ids.ToArray());
        
        entity2.AddComponent(new IndexedEntity { entity = target6 });
        references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 3       },   references5.Ids.ToArray());
        
        entity2.AddComponent(new IndexedEntity { entity = target6 });
        references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] { 3       },   references5.Ids.ToArray());
        
        entity3.RemoveComponent<IndexedEntity>();
        references5 = target5.GetForeignEntities<IndexedEntity>();
        AreEqual(new int [] {         },   references5.Ids.ToArray());
    }
    
    [Test]
    public static void Test_Index_already_added()
    {
        var world   = new EntityStore();
        var entity  = world.CreateEntity(1);
        
        entity.AddComponent(new IndexedName { name = "added" });
        
        var index = (HasValueIndex<string>)world.extension.componentIndexes[StructInfo<IndexedName>.Index];
        index.Add(1, new IndexedName { name = "added" });
        AreEqual(1, index.Count);
    }
    
    [Test]
    public static void Test_Index_already_removed()
    {
        var map = new Dictionary<string, IdArray>();
        var arrayHeap = new IdArrayHeap();
        IndexUtils.RemoveComponentValue(1, "missing", map, arrayHeap);   // add key with default IdArray
        AreEqual(1, map.Count);
    }
    
    private static int[] ToIds(this QueryEntities entities) => entities.ToEntityList().Ids.ToArray();
    
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
        var method          = typeof(Test_Index).GetMethod(nameof(GetIndexedComponentValue), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(T), typeof(V));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetIndexedValue<T,V>), genericMethod);
        return (GetIndexedValue<T,V>)genericDelegate;
    }
    
    internal static V GetIndexedComponentValue<T,V>(in T component) where T : struct, IIndexedComponent<V> {
        return component.GetIndexedValue();
    }
    
    private delegate V GetIndexedValue<T, out V>(in T component) where T : struct, IComponent;
}

}
