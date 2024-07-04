using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index.Query {

internal class IndexContext
{
    internal EntityStore                    store;
    internal ArchetypeQuery                 query1;
    internal ArchetypeQuery                 query2;
    internal ArchetypeQuery                 query3;
    internal ArchetypeQuery                 query4;
    internal ArchetypeQuery                 query5;
    
    internal Entity                         entity0;
    internal Entity                         entity1;
    internal Entity                         entity2;
    internal Entity                         entity3;
    internal Entity                         target;
    
    internal IReadOnlyCollection<string>    nameValues;
    internal IReadOnlyCollection<int>       intValues;
}

public static partial class Test_Index_Query
{
    private static IndexContext Query_Setup()
    {
        var store = new EntityStore();
        var cx = new IndexContext { store = store };
        var targets = new EntityList(store);
        for (int n = 0; n < 10; n++) {
            targets.Add(store.CreateEntity());
        }
        var entities = new List<Entity>();
        for (int n = 0; n < 10; n++) {
            var entity = store.CreateEntity(
                new Position(11 + n, 0, 0),
                new Rotation(),
                new MyComponent1 { a = n },
                new MyComponent2 { b = n },
                new MyComponent3 { b = n },
                new AttackComponent{target = targets[n]});
            entities.Add(entity);
        }
        cx.target   = targets[0];
        cx.entity0  = entities[0];
        cx.entity1  = entities[1];
        cx.entity2  = entities[2];
        cx.entity3  = entities[3];
        
        cx.nameValues  = store.GetAllIndexedComponentValues<IndexedName, string>();
        cx.intValues   = store.GetAllIndexedComponentValues<IndexedInt, int>();
        
        cx.entity0.AddComponent(new IndexedName    { name   = "find-me" });    AreEqual("{ find-me }",  cx.nameValues.Debug());
        cx.entity1.AddComponent(new IndexedInt     { value  = 123       });    AreEqual("{ 123 }",      cx.intValues.Debug());
        cx.entity2.AddComponent(new IndexedName    { name   = "find-me" });    AreEqual("{ find-me }",  cx.nameValues.Debug());
        cx.entity2.AddComponent(new IndexedInt     { value  = 123       });    AreEqual("{ 123 }",      cx.intValues.Debug());
        cx.entity2.AddComponent(new AttackComponent{ target = cx.target });
        cx.entity3.AddComponent(new IndexedInt     { value  = 456       });    AreEqual("{ 123, 456 }", cx.intValues.Debug());
        
        AreNotSame(cx.entity1.Archetype, cx.entity2.Archetype);
        AreEqual  (2, cx.entity1.Archetype.Count); // ensure testing with an archetype containing multiple entities
        return cx;
    }
    
    private static void Query_Assertions(IndexContext cx)
    {
        AreEqual(2, cx.query1.Entities.Count);  AreEqual("{ 11, 13 }",      cx.query1.Entities.Debug());
        AreEqual(2, cx.query2.Entities.Count);  AreEqual("{ 12, 13 }",      cx.query2.Entities.Debug());
        AreEqual(3, cx.query3.Entities.Count);  AreEqual("{ 11, 13, 12 }",  cx.query3.Entities.Debug());
        AreEqual(3, cx.query5.Entities.Count);  AreEqual("{ 12, 13, 14 }",  cx.query5.Entities.Debug());
        
        cx.entity2.RemoveComponent<IndexedName>();                          AreEqual(1, cx.nameValues.Count);
        AreEqual(1, cx.query1.Entities.Count);  AreEqual("{ 11 }",          cx.query1.Entities.Debug());
        AreEqual(2, cx.query2.Entities.Count);  AreEqual("{ 12, 13 }",      cx.query2.Entities.Debug());
        AreEqual(3, cx.query3.Entities.Count);  AreEqual("{ 11, 12, 13 }",  cx.query3.Entities.Debug());
        AreEqual(3, cx.query5.Entities.Count);  AreEqual("{ 12, 13, 14 }",  cx.query5.Entities.Debug());
        
        cx.entity2.RemoveComponent<IndexedInt>();                           AreEqual(2, cx.intValues.Count);
        AreEqual(1, cx.query1.Entities.Count);  AreEqual("{ 11 }",          cx.query1.Entities.Debug());
        AreEqual(1, cx.query2.Entities.Count);  AreEqual("{ 12 }",          cx.query2.Entities.Debug());
        AreEqual(2, cx.query3.Entities.Count);  AreEqual("{ 11, 12 }",      cx.query3.Entities.Debug());
        AreEqual(2, cx.query5.Entities.Count);  AreEqual("{ 12, 14 }",      cx.query5.Entities.Debug());
        
        cx.entity1.RemoveComponent<IndexedInt>();                           AreEqual(1, cx.intValues.Count);
        AreEqual(1, cx.query1.Entities.Count);  AreEqual("{ 11 }",          cx.query1.Entities.Debug());
        AreEqual(0, cx.query2.Entities.Count);  AreEqual("{ }",             cx.query2.Entities.Debug());
        AreEqual(1, cx.query3.Entities.Count);  AreEqual("{ 11 }",          cx.query3.Entities.Debug());
        AreEqual(1, cx.query5.Entities.Count);  AreEqual("{ 14 }",          cx.query5.Entities.Debug());
        
        cx.entity0.RemoveComponent<IndexedName>();                          AreEqual(0, cx.nameValues.Count);
        AreEqual(0, cx.query1.Entities.Count);  AreEqual("{ }",             cx.query1.Entities.Debug());
        AreEqual(0, cx.query2.Entities.Count);  AreEqual("{ }",             cx.query2.Entities.Debug());
        AreEqual(0, cx.query3.Entities.Count);  AreEqual("{ }",             cx.query3.Entities.Debug());
        AreEqual(1, cx.query5.Entities.Count);  AreEqual("{ 14 }",          cx.query5.Entities.Debug());
        
        AreEqual(1, cx.query4.Entities.Count);  AreEqual("{ 13 }",          cx.query4.Entities.Debug());
    }
    
    [Test]
    public static void Test_Index_Component_Add_Remove_Arg0()
    {
        var cx = Query_Setup();
        QueryArg0(cx);
        Query_Assertions(cx);
    }
    
    [Test]
    public static void Test_Index_Component_Add_Remove_Arg1()
    {
        var cx = Query_Setup();
        QueryArg1(cx);
        Query_Assertions(cx);
    }
    
    [Test]
    public static void Test_Index_Component_Add_Remove_Arg2()
    {
        var cx = Query_Setup();
        QueryArg2(cx);
        Query_Assertions(cx);
    }
    
    [Test]
    public static void Test_Index_Component_Add_Remove_Arg3()
    {
        var cx = Query_Setup();
        QueryArg3(cx);
        Query_Assertions(cx);
    }
    
    [Test]
    public static void Test_Index_Component_Add_Remove_Arg4()
    {
        var cx = Query_Setup();
        QueryArg4(cx);
        Query_Assertions(cx);
    }
    
    [Test]
    public static void Test_Index_Component_Add_Remove_Arg5()
    {
        var cx = Query_Setup();
        QueryArg5(cx);
        Query_Assertions(cx);
    }
}

}
