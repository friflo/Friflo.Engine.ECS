using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {


public static class Test_EntityGenericSet
{
    [Test]
    public static void Test_Entity_generic_Set()
    {
        var store = new EntityStore(PidType.UsePidAsId);

        int componentAddedCount = 0;
        Action<ComponentChanged> componentAdded = changed => {
            var str = changed.ToString();
            switch (componentAddedCount++)
            {                   
                // --- entity 1
                case 0:     AreEqual(new Position(2,2,2),       changed.Component<Position>());     
                            AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                            AreEqual("entity: 1 - event > Update Component: [Position]",        str);   break;
                
                // --- entity 2
                case 1:     AreEqual(new Position(2,2,2),       changed.Component<Position>());     
                            AreEqual("entity: 2 - event > Update Component: [Position]",        str);   break;
                case 2:     AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());     
                            AreEqual("entity: 2 - event > Update Component: [Scale3]",          str);   break;
                
                // --- entity 3
                case 3:     AreEqual(new Position(2,2,2),       changed.Component<Position>());     
                            AreEqual("entity: 3 - event > Update Component: [Position]",        str);   break;
                case 4:     AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());     
                            AreEqual("entity: 3 - event > Update Component: [Scale3]",          str);   break;
                case 5:     AreEqual(new EntityName("new"),     changed.Component<EntityName>());     
                    AreEqual("entity: 3 - event > Update Component: [EntityName]",      str);   break;
                
                // --- entity 4
                case 6:     AreEqual(new Position(2,2,2),       changed.Component<Position>());     
                            AreEqual("entity: 4 - event > Update Component: [Position]",        str);   break;
                case 7:     AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());     
                            AreEqual("entity: 4 - event > Update Component: [Scale3]",          str);   break;
                case 8:     AreEqual(new EntityName("new"),     changed.Component<EntityName>());     
                            AreEqual("entity: 4 - event > Update Component: [EntityName]",      str);   break;
                case 9:     AreEqual(new MyComponent1{ a = 2 }, changed.Component<MyComponent1>());     
                            AreEqual("entity: 4 - event > Update Component: [MyComponent1]",    str);   break;
                
                // --- entity 5
                case 10:    AreEqual(new Position(2,2,2),       changed.Component<Position>());     
                            AreEqual("entity: 5 - event > Update Component: [Position]",        str);   break;
                case 11:    AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());     
                            AreEqual("entity: 5 - event > Update Component: [Scale3]",          str);   break;
                case 12:    AreEqual(new EntityName("new"),     changed.Component<EntityName>());     
                            AreEqual("entity: 5 - event > Update Component: [EntityName]",      str);   break;
                case 13:    AreEqual(new MyComponent1{ a = 2 }, changed.Component<MyComponent1>());     
                            AreEqual("entity: 5 - event > Update Component: [MyComponent1]",    str);   break;
                case 14:    AreEqual(new MyComponent2 { b = 2}, changed.Component<MyComponent2>());     
                            AreEqual("entity: 5 - event > Update Component: [MyComponent2]",    str);   break;
            }
        };
        var tag         = Tags.Get<TestTag>();
        var entity1     = store.CreateEntity(1);
        var entity2     = store.CreateEntity(2);
        var entity3     = store.CreateEntity(3);
        var entity4     = store.CreateEntity(4);
        var entity5     = store.CreateEntity(5);
        
        entity1.Add(new Position(1,1,1), tag);
        entity2.Add(new Position(1,1,1), new Scale3(1,1,1), tag);
        entity3.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), tag);
        entity4.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, tag);
        entity5.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, new MyComponent2 { b = 1 }, tag);
        
        store.OnComponentAdded  += componentAdded;
        
        for (int n = 0; n < 2; n++)
        {
            entity1.Set(new Position(2,2,2));
            entity2.Set(new Position(2,2,2), new Scale3(2,2,2));
            entity3.Set(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"));
            entity4.Set(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), new MyComponent1 { a = 2 });
            entity5.Set(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), new MyComponent1 { a = 2 }, new MyComponent2 { b = 2 });
            
            store.OnComponentAdded  -= componentAdded;
        }
        AreEqual(15, componentAddedCount);
    }
    
    [Test]
    public static void Test_Entity_generic_Set_6_and_more()
    {
        var store = new EntityStore(PidType.UsePidAsId);
        
        var entities = new Entity[5];
        for (int n = 0; n < 5; n++) {
            entities[n] = store.CreateEntity(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6(), new MyComponent7());    
        }
        var componentEventCount = 0;
        store.OnComponentAdded  += _ => { componentEventCount++; };
        
        entities[0].Set(new Position(1,1,1), new Scale3(1,1,1), new Rotation(1,1,1,1), new MyComponent1{a=1}, new MyComponent2{b=1}, new MyComponent3{b=1});
        entities[1].Set(new Position(1,1,1), new Scale3(1,1,1), new Rotation(1,1,1,1), new MyComponent1{a=1}, new MyComponent2{b=1}, new MyComponent3{b=1}, new MyComponent4{b=1});
        entities[2].Set(new Position(1,1,1), new Scale3(1,1,1), new Rotation(1,1,1,1), new MyComponent1{a=1}, new MyComponent2{b=1}, new MyComponent3{b=1}, new MyComponent4{b=1}, new MyComponent5{b=1});
        entities[3].Set(new Position(1,1,1), new Scale3(1,1,1), new Rotation(1,1,1,1), new MyComponent1{a=1}, new MyComponent2{b=1}, new MyComponent3{b=1}, new MyComponent4{b=1}, new MyComponent5{b=1}, new MyComponent6{b=1});
        entities[4].Set(new Position(1,1,1), new Scale3(1,1,1), new Rotation(1,1,1,1), new MyComponent1{a=1}, new MyComponent2{b=1}, new MyComponent3{b=1}, new MyComponent4{b=1}, new MyComponent5{b=1}, new MyComponent6{b=1}, new MyComponent7{b=1});
        
        for (int n = 0; n < 5; n++) {
            var entity = entities[n];
            AreEqual(new Position(1,1,1),   entity.GetComponent<Position>());
            AreEqual(new Scale3  (1,1,1),   entity.GetComponent<Scale3>());
            AreEqual(new Rotation(1,1,1,1), entity.GetComponent<Rotation>());
            AreEqual(new MyComponent1{a=1}, entity.GetComponent<MyComponent1>());
            AreEqual(new MyComponent2{b=1}, entity.GetComponent<MyComponent2>());
            AreEqual(new MyComponent3{b=1}, entity.GetComponent<MyComponent3>());
            
            if (n > 0) AreEqual(new MyComponent4{b=1}, entity.GetComponent<MyComponent4>());
            if (n > 1) AreEqual(new MyComponent5{b=1}, entity.GetComponent<MyComponent5>());
            if (n > 2) AreEqual(new MyComponent6{b=1}, entity.GetComponent<MyComponent6>());
            if (n > 3) AreEqual(new MyComponent7{b=1}, entity.GetComponent<MyComponent7>());
        }
        
        AreEqual(40, componentEventCount);
    }
    
    [Test]
    public static void Test_Entity_generic_Set_exception()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        store.OnComponentAdded += _ => { }; 
        var entity  = store.CreateEntity(new EntityName(), new Position());
        
        var e = Throws<MissingComponentException>(() => {
            entity.Set(new Scale3());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Scale3(), new Rotation());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1, MyComponent2]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5, MyComponent6]", e!.Message);
        
        e = Throws<MissingComponentException>(() => {
            entity.Set(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6(), new MyComponent7());    
        });
        AreEqual("entity id: 1  [EntityName, Position] - missing: [Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5, MyComponent6, MyComponent7]", e!.Message);
        
    }
    
    [Test]
    public static void Test_Entity_generic_Set_Perf()
    {
        int count = 10; // 100_000_000 ~ #PC: 1231 ms
        var store = new EntityStore(PidType.UsePidAsId);
        var entity = store.CreateEntity();
        
        entity.Add(new Position(), new Rotation(), new EntityName(), new MyComponent1(), new MyComponent2());
        entity.Set(new Position(), new Rotation(), new EntityName(), new MyComponent1(), new MyComponent2());
        
        var sw = new Stopwatch();
        sw.Start();
        var start = Mem.GetAllocatedBytes();
        
        for (int n = 0; n < count; n++) {
            entity.Set(new Position(), new Rotation(), new EntityName(), new MyComponent1(), new MyComponent2());
        }
        Mem.AssertNoAlloc(start);
        Console.WriteLine($"Entity.Set<>() - duration: {sw.ElapsedMilliseconds} ms");
    }
}

}