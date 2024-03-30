using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {


public static class Test_Entity_generic
{
    [Test]
    public static void Test_Entity_generic_Add()
    {
        var store   = new EntityStore();
        
        int tagsCount = 0;
        Action<TagsChanged> tagsChanged = changed => {
            var str = changed.ToString();
            switch (tagsCount++)
            {
                case 0: AreEqual("entity: 1 - event > Add Tags: [#TestTag]", str); break;
                case 1: AreEqual("entity: 2 - event > Add Tags: [#TestTag]", str); break;
                case 2: AreEqual("entity: 3 - event > Add Tags: [#TestTag]", str); break;
                case 3: AreEqual("entity: 4 - event > Add Tags: [#TestTag]", str); break;
                case 4: AreEqual("entity: 5 - event > Add Tags: [#TestTag]", str); break;
                case 5: AreEqual("entity: 101 - event > Add Tags: [#TestTag]", str); break;
                case 6: AreEqual("entity: 101 - event > Add Tags: [#TestTag2]", str); break;
            }
        };
        int componentAddedCount = 0;
        Action<ComponentChanged> componentAdded = changed => {
            var str = changed.ToString();
            switch (componentAddedCount++)
            {                   
                // --- entity 1
                case 0:     AreEqual("entity: 1 - event > Add Component: [Position]",       str);   break;
                case 1:     AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                            AreEqual("entity: 1 - event > Update Component: [Position]",    str);   break;
                
                // --- entity 2
                case 2:     AreEqual("entity: 2 - event > Add Component: [Position]",       str);   break;
                case 3:     AreEqual("entity: 2 - event > Add Component: [Scale3]",         str);   break;
                case 4:     AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                            AreEqual(new Position(2,2,2),       changed.Component<Position>());
                            AreEqual("entity: 2 - event > Update Component: [Position]",    str);   break;
                case 5:     AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                            AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());
                            AreEqual("entity: 2 - event > Update Component: [Scale3]",      str);   break;
                    
                // --- entity 3
                case 6:     AreEqual("entity: 3 - event > Add Component: [EntityName]",     str);   break;
                case 7:     AreEqual("entity: 3 - event > Add Component: [Position]",       str);   break;
                case 8:     AreEqual("entity: 3 - event > Add Component: [Scale3]",         str);   break;
                case 9:     AreEqual(new EntityName("old"),     changed.OldComponent<EntityName>());
                            AreEqual(new EntityName("new"),     changed.Component<EntityName>());
                            AreEqual("entity: 3 - event > Update Component: [EntityName]",  str);   break;
                case 10:    AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                            AreEqual(new Position(2,2,2),       changed.Component<Position>());
                            AreEqual("entity: 3 - event > Update Component: [Position]",    str);   break;
                case 11:    AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                            AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());
                            AreEqual("entity: 3 - event > Update Component: [Scale3]",      str);   break;
                
                // --- entity 4
                case 12:    AreEqual("entity: 4 - event > Add Component: [EntityName]",     str);   break;
                case 13:    AreEqual("entity: 4 - event > Add Component: [Position]",       str);   break;
                case 14:    AreEqual("entity: 4 - event > Add Component: [Scale3]",         str);   break;
                case 15:    AreEqual("entity: 4 - event > Add Component: [MyComponent1]",   str);   break;
                case 16:    AreEqual(new EntityName("old"),     changed.OldComponent<EntityName>());
                            AreEqual(new EntityName("new"),     changed.Component<EntityName>());
                            AreEqual("entity: 4 - event > Update Component: [EntityName]",  str);   break;
                case 17:    AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                            AreEqual(new Position(2,2,2),       changed.Component<Position>());
                            AreEqual("entity: 4 - event > Update Component: [Position]",    str);   break;
                case 18:    AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                            AreEqual(new Scale3(2,2,2),         changed.Component<Scale3>());
                            AreEqual("entity: 4 - event > Update Component: [Scale3]",      str);   break;
                case 19:    AreEqual(new MyComponent1{ a = 1 }, changed.OldComponent<MyComponent1>());
                            AreEqual(new MyComponent1{ a = 2 }, changed.Component<MyComponent1>());
                            AreEqual("entity: 4 - event > Update Component: [MyComponent1]",str);   break;
                
                // --- entity 101
                case 30:    AreEqual("entity: 101 - event > Add Component: [Position]",     str);   break;
                case 31:    AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                            AreEqual(new Position(2,2,2),       changed.Component<Position>());
                            AreEqual("entity: 101 - event > Update Component: [Position]",  str);   break;
                case 32:    AreEqual("entity: 101 - event > Add Component: [Scale3]",       str);   break;
            }
        };
        store.OnTagsChanged     += tagsChanged;
        store.OnComponentAdded  += componentAdded;
        
        for (int n = 0; n < 2; n++)
        {
            var tag         = Tags.Get<TestTag>();
            var tag2        = Tags.Get<TestTag2>();
            var entity1     = store.CreateEntity(1);
            var entity2     = store.CreateEntity(2);
            var entity3     = store.CreateEntity(3);
            var entity4     = store.CreateEntity(4);
            var entity5     = store.CreateEntity(5);
            var entity101   = store.CreateEntity(101);
            
            
            entity1.Add(new Position(1,1,1), tag);
            entity1.Add(new Position(2,2,2), tag);
            AreEqual("id: 1  [Position, #TestTag]", entity1.ToString());
            
            entity2.Add(new Position(1,1,1), new Scale3(1,1,1), tag);
            entity2.Add(new Position(2,2,2), new Scale3(2,2,2), tag);
            AreEqual("id: 2  [Position, Scale3, #TestTag]", entity2.ToString());
            
            entity3.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), tag);
            entity3.Add(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), tag);
            AreEqual("id: 3  \"new\"  [EntityName, Position, Scale3, #TestTag]", entity3.ToString());
            
            entity4.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, tag);
            entity4.Add(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), new MyComponent1 { a = 2 }, tag);
            AreEqual("id: 4  \"new\"  [EntityName, Position, Scale3, MyComponent1, #TestTag]", entity4.ToString());
            
            entity5.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, new MyComponent2 { b = 1 }, tag);
            entity5.Add(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), new MyComponent1 { a = 2 }, new MyComponent2 { b = 2 }, tag);
            AreEqual("id: 5  \"new\"  [EntityName, Position, Scale3, MyComponent1, MyComponent2, #TestTag]", entity5.ToString());
            
            entity101.Add(new Position(1,1,1), tag);
            AreEqual("id: 101  [Position, #TestTag]", entity101.ToString());
            entity101.Add(new Position(2,2,2), new Scale3(2,2,2), tag2);
            AreEqual("id: 101  [Position, Scale3, #TestTag, #TestTag2]", entity101.ToString());
            
            store.OnTagsChanged     -= tagsChanged;
            store.OnComponentAdded  -= componentAdded;
            
            entity1.DeleteEntity();
            entity2.DeleteEntity();
            entity3.DeleteEntity();
            entity4.DeleteEntity();
            entity5.DeleteEntity();
            entity101.DeleteEntity();
        }
        
        AreEqual(7,  tagsCount);
        AreEqual(33, componentAddedCount);
    }
    
    
    [Test]
    public static void Test_Entity_generic_Remove()
    {
        var store   = new EntityStore();
        
        int tagsCount = 0;
        Action<TagsChanged> tagsChanged = changed => {
            var str = changed.ToString();
            switch (tagsCount++)
            {
                case 0: AreEqual("entity: 1 - event > Remove Tags: [#TestTag]", str); break;
                case 1: AreEqual("entity: 2 - event > Remove Tags: [#TestTag]", str); break;
                case 2: AreEqual("entity: 3 - event > Remove Tags: [#TestTag]", str); break;
                case 3: AreEqual("entity: 4 - event > Remove Tags: [#TestTag]", str); break;
                case 4: AreEqual("entity: 5 - event > Remove Tags: [#TestTag]", str); break;
                case 5: AreEqual("entity: 102 - event > Remove Tags: [#TestTag]", str); break;
            }
        };
        int componentRemovedCount = 0;
        Action<ComponentChanged> componentRemoved = changed => {
            var str = changed.ToString();
            switch (componentRemovedCount++)
            {                   
                // --- entity 1
                case 0: AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                        AreEqual("entity: 1 - event > Remove Component: [Position]",    str); break;
                
                // --- entity 2
                case 1: AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                        AreEqual("entity: 2 - event > Remove Component: [Position]",    str); break;
                case 2: AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                        AreEqual("entity: 2 - event > Remove Component: [Scale3]",      str); break;
                
                // --- entity 3
                case 3: AreEqual(new EntityName("old"),     changed.OldComponent<EntityName>());
                    AreEqual("entity: 3 - event > Remove Component: [EntityName]",  str); break;
                case 4: AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                        AreEqual("entity: 3 - event > Remove Component: [Position]",    str); break;
                case 5: AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                        AreEqual("entity: 3 - event > Remove Component: [Scale3]",      str); break;
                
                // --- entity 4
                case 6: AreEqual(new EntityName("old"),     changed.OldComponent<EntityName>());
                        AreEqual("entity: 4 - event > Remove Component: [EntityName]",  str); break;
                case 7: AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                        AreEqual("entity: 4 - event > Remove Component: [Position]",    str); break;
                case 8: AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                        AreEqual("entity: 4 - event > Remove Component: [Scale3]",      str); break;
                case 9: AreEqual(new MyComponent1{ a = 1 }, changed.OldComponent<MyComponent1>());
                        AreEqual("entity: 4 - event > Remove Component: [MyComponent1]",str); break;

                // --- entity 5
                case 10:AreEqual(new EntityName("old"),     changed.OldComponent<EntityName>());
                        AreEqual("entity: 5 - event > Remove Component: [EntityName]",  str); break;
                case 11:AreEqual(new Position(1,1,1),       changed.OldComponent<Position>());
                        AreEqual("entity: 5 - event > Remove Component: [Position]",    str); break;
                case 12:AreEqual(new Scale3(1,1,1),         changed.OldComponent<Scale3>());
                        AreEqual("entity: 5 - event > Remove Component: [Scale3]",      str); break;
                case 13:AreEqual(new MyComponent1{ a = 1 }, changed.OldComponent<MyComponent1>());
                        AreEqual("entity: 5 - event > Remove Component: [MyComponent1]",str); break;
                case 14:AreEqual(new MyComponent2{ b = 1 }, changed.OldComponent<MyComponent2>());
                        AreEqual("entity: 5 - event > Remove Component: [MyComponent2]",str); break;
                
                // --- entity 102
                case 15: AreEqual("entity: 102 - event > Remove Component: [Scale3]",    str); break;
                case 16: AreEqual("entity: 102 - event > Remove Component: [Scale3]",    str); break;
                case 17: AreEqual("entity: 102 - event > Remove Component: [Scale3]",    str); break;

            }
        };
        
        var tag         = Tags.Get<TestTag>();
        var tags2       = Tags.Get<TestTag, TestTag2>();
        var entity1     = store.CreateEntity();
        var entity2     = store.CreateEntity();
        var entity3     = store.CreateEntity();
        var entity4     = store.CreateEntity();
        var entity5     = store.CreateEntity();
        var entity102   = store.CreateEntity(102);
            
        entity1.Add(new Position(1,1,1), tag);
        entity2.Add(new Position(1,1,1), new Scale3(1,1,1), tag);
        entity3.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), tag);
        entity4.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, tag);
        entity5.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, new MyComponent2 { b = 1 }, tag);
        entity102.Add(new Position(1,1,1), new Scale3(1,1,1), tags2);
        
        store.OnTagsChanged         += tagsChanged;
        store.OnComponentRemoved    += componentRemoved;
        
        for (int n = 0; n < 2; n++) {
            entity1.Remove<Position>(tag);
            AreEqual("id: 1  []", entity1.ToString());
            
            entity2.Remove<Position, Scale3>(tag);
            AreEqual("id: 2  []", entity2.ToString());
            
            entity3.Remove<Position, Scale3, EntityName>(tag);
            AreEqual("id: 3  []", entity3.ToString());
            
            entity4.Remove<Position, Scale3, EntityName, MyComponent1>(tag);
            AreEqual("id: 4  []", entity4.ToString());
            
            entity5.Remove<Position, Scale3, EntityName, MyComponent1, MyComponent2>(tag);
            AreEqual("id: 5  []", entity5.ToString());
            
            entity102.Remove<Scale3>(tag); // cover EntityStoreBase.GetArchetypeRemove()
            AreEqual("id: 102  [Position, #TestTag2]", entity102.ToString());
            
            store.OnTagsChanged     -= tagsChanged;
            store.OnComponentRemoved-= componentRemoved;
        }

        AreEqual(6,  tagsCount);
        AreEqual(16, componentRemovedCount);
    }
}

}