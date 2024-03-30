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
            }
        };
        store.OnTagsChanged     += tagsChanged;
        store.OnComponentAdded  += componentAdded;
        
        for (int n = 0; n < 2; n++)
        {
            var tags    = Tags.Get<TestTag>();
            var entity1  = store.CreateEntity();
            var entity2  = store.CreateEntity();
            var entity3  = store.CreateEntity();
            var entity4  = store.CreateEntity();
            var entity5  = store.CreateEntity();
            
            
            entity1.Add(new Position(1,1,1), tags);
            entity1.Add(new Position(2,2,2), tags);
            
            entity2.Add(new Position(1,1,1), new Scale3(1,1,1), tags);
            entity2.Add(new Position(2,2,2), new Scale3(2,2,2), tags);
            
            entity3.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), tags);
            entity3.Add(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), tags);
            
            entity4.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, tags);
            entity4.Add(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), new MyComponent1 { a = 2 }, tags);
            
            entity5.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, new MyComponent2 { b = 1 }, tags);
            entity5.Add(new Position(2,2,2), new Scale3(2,2,2), new EntityName("new"), new MyComponent1 { a = 2 }, new MyComponent2 { b = 2 }, tags);
            
            store.OnTagsChanged     -= tagsChanged;
            store.OnComponentAdded  -= componentAdded;
        }
        
        AreEqual(5,  tagsCount);
        AreEqual(30, componentAddedCount);
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

                // --- entity 4
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
                case 15: AreEqual(new Scale3(1,1,1),        changed.OldComponent<Scale3>());
                    AreEqual("entity: 102 - event > Remove Component: [Scale3]",    str); break;

            }
        };
        
        var tags     = Tags.Get<TestTag>();
        var entity1  = store.CreateEntity();
        var entity2  = store.CreateEntity();
        var entity3  = store.CreateEntity();
        var entity4  = store.CreateEntity();
        var entity5  = store.CreateEntity();
        var entity102= store.CreateEntity(102);
            
        entity1.Add(new Position(1,1,1), tags);
        entity2.Add(new Position(1,1,1), new Scale3(1,1,1), tags);
        entity3.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), tags);
        entity4.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, tags);
        entity5.Add(new Position(1,1,1), new Scale3(1,1,1), new EntityName("old"), new MyComponent1 { a = 1 }, new MyComponent2 { b = 1 }, tags);
        entity102.Add(new Position(1,1,1), new Scale3(1,1,1), tags);
        
        store.OnTagsChanged         += tagsChanged;
        store.OnComponentRemoved    += componentRemoved;
        
        for (int n = 0; n < 2; n++) {
            entity1.Remove<Position>(tags);
            entity2.Remove<Position, Scale3>(tags);
            entity3.Remove<Position, Scale3, EntityName>(tags);
            entity4.Remove<Position, Scale3, EntityName, MyComponent1>(tags);
            entity5.Remove<Position, Scale3, EntityName, MyComponent1, MyComponent2>(tags);
            entity102.Remove<Scale3>(tags); // cover EntityStoreBase.GetArchetypeRemove()
            
            store.OnTagsChanged     -= tagsChanged;
            store.OnComponentRemoved-= componentRemoved;
        }

        AreEqual(6,  tagsCount);
        AreEqual(16, componentRemovedCount);
    }
}

}