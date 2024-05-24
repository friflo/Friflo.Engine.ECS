using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;
using static Friflo.Engine.ECS.StoreOwnership;
using static Friflo.Engine.ECS.TreeMembership;
using static Friflo.Engine.ECS.NodeFlags;
using static Tests.Utils.Events;

// ReSharper disable ConvertToLocalFunction
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_Entity_Tree
{
    [Test]
    public static void Test_CreateEntity_UseRandomPids()
    {
        var store   = new EntityStore(PidType.RandomPids);
        store.SetRandomSeed(0);
        var entity  = store.CreateEntity();
        AreEqual(1,             entity.Id);
        AreEqual(1559595546L,   entity.Pid);
        AreEqual(1,             store.PidToId(1559595546L));
        AreEqual(1,             store.GetEntityByPid(1559595546L).Id);
    }
    
    [Test]
    public static void Test_CreateEntity_UsePidAsId()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        AreEqual(1,     entity.Id);
        AreEqual(1,     entity.Pid);
        AreEqual(1,     store.PidToId(1L));
        AreEqual(1,     store.GetEntityByPid(1L).Pid);
    }
    
    [Test]
    public static void Test_AddChild()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
       
        IsTrue  (root.Parent.IsNull);
        AreEqual(0,         root.ChildEntities.Ids.Length);
        AreEqual(0,         root.ChildCount);
        AreEqual(attached,  root.StoreOwnership);
        foreach (var _ in root.ChildEntities) {
            Fail("expect empty child nodes");
        }
        
        // -- add child
        var child = store.CreateEntity(4);
        AreEqual(4,         child.Id);
        child.AddComponent(new EntityName("child"));
        AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 4", args.ToString()));
        AreEqual(0,             root.AddChild(child));
        AreEqual("Entity[1]",   root.ChildEntities.ToString());
        AreEqual("entities: 2", store.ToString());
        
        IsTrue(root ==          child.Parent);
        AreEqual(attached,      child.StoreOwnership);
        var childEntities =     root.ChildEntities;
        AreEqual(1,             childEntities.Ids.Length);
        AreEqual(4,             childEntities.Ids[0]);
        AreEqual(1,             root.ChildCount);
        AreEqual(1,             childEntities.Count);
        IsTrue(child ==         childEntities[0]);
        int count = 0;
        foreach (var childEntity in root.ChildEntities) {
            count++;
            IsTrue(child == childEntity);
        }
        AreEqual(1,         count);
        
        // -- add same child again
        AreEqual(-1,        root.AddChild(child));       // event handler is not called
        AreEqual(1,         childEntities.Ids.Length);
        var rootNode = root.GetComponent<TreeNode>();
        AreEqual(1,         rootNode.ChildCount);
        AreEqual(1,         rootNode.ChildIds.Length);
    //  AreEqual("id: 1  \"root\"  [EntityName]  ChildCount: 1  flags: Created",  root.ToString()); TREE_NODE
        AreEqual("id: 1  \"root\"  [EntityName, TreeNode]",  root.ToString());
        AreEqual(1,         childEntities.Count);
        IsTrue(child ==     childEntities[0]);
        
        // --- copy child Entity's to array
        var array = new Entity[childEntities.Count];
        childEntities.ToArray(array);
        IsTrue(child == array[0]);
    }
    
    [Test]
    public static void Test_InsertChild()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
       
        IsTrue  (root.Parent.IsNull);
        AreEqual(0,         root.ChildEntities.Ids.Length);
        AreEqual(0,         root.ChildCount);
        AreEqual(attached,  root.StoreOwnership);
        foreach (var _ in root.ChildEntities) {
            Fail("expect empty child nodes");
        }
        var child4 = store.CreateEntity(4);
        {
            // -- insert new child (id: 4)
            AreEqual(4,         child4.Id);
            var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 4", args.ToString()));
            root.InsertChild(0, child4);
            AreEqual(1, events.Seq);
            
            IsTrue(root ==      child4.Parent);
            AreEqual(attached,  child4.StoreOwnership);
            var childNodes =    root.ChildEntities;
            AreEqual(1,         childNodes.Ids.Length);
            AreEqual(4,         childNodes.Ids[0]);
            AreEqual(1,         root.ChildCount);
            IsTrue(child4 ==    childNodes[0]);
            int count = 0;
            foreach (var child in childNodes) {
                count++;
                IsTrue(child4 == child);
            }
            AreEqual(1,         count);
            
            // --- insert same child (id: 4) at same index again
            root.InsertChild(0, child4);     // event handler is not called
            AreEqual(1,                                 childNodes.Ids.Length);
            var rootNode = root.GetComponent<TreeNode>();
            AreEqual(1,                                 rootNode.ChildCount);
            AreEqual(1,                                 rootNode.ChildIds.Length);
            AreEqual("id: 1  \"root\"  [EntityName, TreeNode]",  root.ToString());
            IsTrue(child4 ==                            childNodes[0]);
            events.RemoveHandler();
        }
        var child5 = store.CreateEntity(5);
        {
            // --- insert new child (id: 5) at index 0
            AreEqual(5,         child5.Id);
            var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 5", args.ToString()));
            root.InsertChild(0, child5);
            AreEqual(1, events.Seq);
            
            IsTrue(root ==      child5.Parent);
            AreEqual(attached,  child5.StoreOwnership);
            var childNodes =    root.ChildEntities;
            AreEqual(2,         childNodes.Ids.Length);
            AreEqual(5,         childNodes.Ids[0]);
            AreEqual(4,         childNodes.Ids[1]);
            AreEqual(2,         root.ChildCount);
            IsTrue(child5 ==    childNodes[0]);
            IsTrue(child4 ==    childNodes[1]);
            var count = 0;
            foreach (var child in childNodes) {
                switch(count++) {
                    case 0:     IsTrue(child5 == child);    break;
                    case 1:     IsTrue(child4 == child);    break;
                    default:    Fail("unexpected");         return;
                }
            }
            AreEqual(2,         count);
        }
        // --- copy child Entity's to array
        {
            var childNodes =    root.ChildEntities;
            var array = new Entity[childNodes.Count];
            childNodes.ToArray(array);
            IsTrue(child5 == array[0]);
            IsTrue(child4 == array[1]);
        }
    }
    
    /// <summary>Cover <see cref="EntityStore.EnsureChildIdsCapacity"/></summary>
    [Test]
    public static void Test_InsertChild_EnsureChildIdsCapacity()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var events = SetHandlerSeq(store, (args, seq) => {
            AreEqual(1,                                 args.EntityId);
            AreEqual(1,                                 args.Entity.Id);
            AreSame (store,                             args.Store);
            AreEqual(seq,                               args.ChildIndex);
            AreEqual(seq + 2,                           args.ChildId);
            AreEqual(seq + 2,                           args.Child.Id);
            AreEqual(ChildEntitiesChangedAction.Add,    args.Action);
            AreEqual(seq + 1,                           root.ChildCount);
        });
        for (int n = 0; n < 100; n++) {
            var child = store.CreateEntity(n + 2);
            root.InsertChild(n, child);
        }
        AreEqual(100, events.Seq);
    }
    
    /// <summary>Cover <see cref="EntityStore.InsertChild"/></summary>
    [Test]
    public static void Test_InsertChild_cover()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
       
        var child2      = store.CreateEntity(2);
        var child3      = store.CreateEntity(3);
        var subChild4   = store.CreateEntity(4);
        
        AreEqual(0,             root.AddChild(child2));
        AreEqual(1,             root.AddChild(child3));
        AreEqual(0,             child2.AddChild(subChild4));
        AreEqual("Entity[2]",   root.ChildEntities.ToString());
        AreEqual("entities: 4", store.ToString());
        
        var events = SetHandlerSeq(store, (args, seq) => {
            switch (seq) {
                case 0:     AreEqual("entity: 1 - event > Remove Child[1] = 3",    args.ToString()); return;
                case 1:     AreEqual("entity: 1 - event > Add Child[0] = 3",       args.ToString()); return;
            }
        });
        AreEqual(new [] { 2, 3 }, root.ChildIds.ToArray());
        AreEqual(1, root.GetChildIndex(child3));
        root.InsertChild(0, child3);    // move child3 within root. index: 1 -> 0
        AreEqual(2, events.Seq);
        AreEqual(0, root.GetChildIndex(child3));
        AreEqual(new [] { 3, 2 }, root.ChildIds.ToArray());
        
        events = SetHandlerSeq(store, (args, seq) => {
            switch (seq) {
                case 0:     AreEqual("entity: 2 - event > Remove Child[0] = 4",    args.ToString()); return;
                case 1:     AreEqual("entity: 1 - event > Add Child[0] = 4",       args.ToString()); return;
            }
        });
        root.InsertChild(0, subChild4);    // change subChild4 parent: child2 -> root
        AreEqual(2, events.Seq);
        AreEqual(new [] { 4, 3, 2 }, root.ChildIds.ToArray());
        
        Throws<IndexOutOfRangeException>(() => {
            root.InsertChild(100, subChild4);
        });
    }
    
    /// <summary>code coverage for <see cref="EntityStore.SetTreeFlags"/></summary>
    [Test]
    public static void Test_AddChild_move_root_tree_entity()
    {
        var store       = new EntityStore(PidType.RandomPids);
        var root        = store.CreateEntity(1);
        store.SetStoreRoot(root);
        var child1      = store.CreateEntity(2);
        var child2      = store.CreateEntity(3);
        var subChild    = store.CreateEntity(4);
        
        var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 2", args.ToString()));
        AreEqual(0, root.AddChild(child1));
        AreEqual(1, events.Seq);
        events.RemoveHandler();
        
        events = AddHandler(store, args => AreEqual("entity: 2 - event > Add Child[0] = 4", args.ToString()));
        AreEqual(0, child1.AddChild(subChild));
        AreEqual(1, events.Seq);
        events.RemoveHandler();
        
        events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[1] = 3", args.ToString()));
        AreEqual(1, root.AddChild(child2));
        AreEqual(1, events.Seq);
        AreEqual(1, child1.ChildCount);
        AreEqual(0, child2.ChildCount);
        events.RemoveHandler();
        
        events = SetHandlerSeq(store, (args, seq) => {
            switch (seq) {
                case 0:     AreEqual("entity: 2 - event > Remove Child[0] = 4",    args.ToString()); return;
                case 1:     AreEqual("entity: 3 - event > Add Child[0] = 4",       args.ToString()); return;
            }
        });
        AreEqual(0, child2.AddChild(subChild));  // subChild is moved from child1 to child2
        AreEqual(2, events.Seq);
        AreEqual(0, child1.ChildCount);
        AreEqual(1, child2.ChildCount);
    }
    
    [Test]
    public static void Test_RemoveChild()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var child   = store.CreateEntity(2);
        AreEqual(floating,  root.TreeMembership);
        AreEqual(floating,  child.TreeMembership);
        
        var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 2",     args.ToString()));
        AreEqual(0, root.AddChild(child));
        AreEqual(1, events.Seq);
        
        store.SetStoreRoot(root);
        IsTrue  (root.Parent.IsNull);
        NotNull (child.Parent);
        AreEqual(treeNode,  root.TreeMembership);
        AreEqual(treeNode,  child.TreeMembership);
        events.RemoveHandler();
        
        // --- remove child
        events = AddHandler(store, args => AreEqual("entity: 1 - event > Remove Child[0] = 2",  args.ToString()));
        IsTrue  (root.RemoveChild(child));
        AreEqual(1, events.Seq);
        AreEqual(treeNode,  root.TreeMembership);
        AreEqual(floating,  child.TreeMembership);
        AreEqual(0,         root.ChildCount);
        IsTrue  (child.Parent.IsNull);
        
        // --- remove same child again
        IsFalse (root.RemoveChild(child));        // event handler is not called
        
        AreEqual(0,         root.ChildCount);
        IsTrue  (child.Parent.IsNull);
        AreEqual(2,         store.Count);
    }
    
    [Test]
    public static void Test_RemoveChild_from_multiple_children()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        var child4  = store.CreateEntity(4);
        var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 2",     args.ToString()));
        AreEqual(0, root.AddChild(child2));
        AreEqual(1, events.Seq);
        events.RemoveHandler();
        
        events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[1] = 3",     args.ToString()));
        AreEqual(1, root.AddChild(child3));
        AreEqual(1, events.Seq);
        events.RemoveHandler();
        
        events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[2] = 4",     args.ToString()));
        AreEqual(2, root.AddChild(child4));
        AreEqual(1, events.Seq);
        events.RemoveHandler();
        
        events = AddHandler(store, args => AreEqual("entity: 1 - event > Remove Child[1] = 3",  args.ToString()));
        IsTrue  (root.RemoveChild(child3));
        AreEqual(1, events.Seq);
        var childIds = root.ChildIds; 
        AreEqual(2, childIds.Length);
        AreEqual(2, childIds[0]);
        AreEqual(4, childIds[1]);
    }
    
    [Test]
    public static void Test_SetRoot()
    {
        var store   = new EntityStore(PidType.RandomPids);
        
        var root    = store.CreateEntity(1);
        IsTrue (root.Parent.IsNull);
        
        store.SetStoreRoot(root);

        IsTrue(root ==          store.StoreRoot);
        IsTrue  (root.Parent.IsNull);
        
        var child   = store.CreateEntity(2);
        AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 2",     args.ToString()));
        AreEqual(0,             root.AddChild(child));
        IsTrue(root ==          child.Parent);
        AreEqual(treeNode,      child.TreeMembership);
        AreEqual(2,             store.Count);
        var node0 = store.GetEntityNode(0);
        var node2 = store.GetEntityNode(2);
        AreEqual("id: 0",                                   node0.ToString());
        AreEqual("id: 2  []  flags: TreeNode | Created",    node2.ToString());
        
        AreEqual(NullNode,                                  node0.Flags);
        AreEqual(NodeFlags.TreeNode | Created,                        node2.Flags);
    }
    
    [Test]
    public static void Test_move_with_AddChild()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var child   = store.CreateEntity(3);
        entity1.AddComponent(new EntityName("entity-1"));
        entity2.AddComponent(new EntityName("entity-2"));
        child.AddComponent  (new EntityName("child"));
        
        var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 3", args.ToString()));
        AreEqual(0,     entity1.AddChild(child));
        AreEqual(1,     entity1.ChildCount);
        
        var e = Throws<InvalidOperationException>(() => {
            entity1.AddChild(entity1);
        });
        AreEqual("Cannot add entity to itself as a child. id: 1", e!.Message);
        AreEqual(1,     entity1.ChildCount); // count stays unchanged
        events.RemoveHandler();
        
        // --- move child from entity1 -> entity2
        events = SetHandlerSeq(store, (args, seq) => {
            switch (seq) {
                case 0:     AreEqual("entity: 1 - event > Remove Child[0] = 3",    args.ToString()); return;
                case 1:     AreEqual("entity: 2 - event > Add Child[0] = 3",       args.ToString()); return;
            }
        });
        AreEqual(0,     entity2.AddChild(child));
        AreEqual(2,     events.Seq);
        AreEqual(0,     entity1.ChildCount);
        AreEqual(1,     entity2.ChildCount);
        IsTrue(child == entity2.ChildEntities[0]);
    }
    
    [Test]
    public static void Test_Entity_ToString()
    {
        var nullEntity = new Entity();
        AreEqual("null",                nullEntity.ToString());
        
        var store   = new EntityStore(PidType.RandomPids);
        
        var entity  = store.CreateEntity(1);
        AreEqual("id: 1  []",           entity.ToString());
        
        entity.AddComponent<EntityName>();
        AreEqual("id: 1  [EntityName]", entity.ToString());
        
        entity.DeleteEntity();
        AreEqual("id: 1  (detached)",   entity.ToString());
    }
    
    [Test]
    public static void Test_DeleteEntity()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
        store.SetStoreRoot(root);
        var child   = store.CreateEntity(2);
        AreEqual(attached,  child.StoreOwnership);
        child.AddComponent(new EntityName("child"));
        var events = AddHandler(store, args => AreEqual("entity: 1 - event > Add Child[0] = 2", args.ToString()));
        AreEqual(0, root.AddChild(child));
        AreEqual(1, events.Seq);
        var subChild = store.CreateEntity(3);
        IsTrue(root ==      subChild.Store.StoreRoot);
        subChild.AddComponent(new EntityName("subChild"));
        events.RemoveHandler();
        
        events = AddHandler(store, args => AreEqual("entity: 2 - event > Add Child[0] = 3", args.ToString()));
        AreEqual(0, child.AddChild(subChild));
        AreEqual(1, events.Seq);
        
        AreEqual(3,         store.Count);
        IsTrue(root ==      child.Parent);
        IsTrue(root ==      subChild.Store.StoreRoot);
        var childArchetype = child.Archetype;
        AreEqual(2,         childArchetype.Count);
        AreEqual(treeNode,  subChild.TreeMembership);
        NotNull (child.Archetype);
        NotNull (child.Store);
        
        
        var start = Mem.GetAllocatedBytes();
        events.RemoveHandler();
        child.DeleteEntity();
        Mem.AssertNoAlloc(start);
        AreEqual(1,         childArchetype.Count);
        AreEqual(2,         store.Count);
        AreEqual(0,         root.ChildCount);
        AreEqual(floating,  subChild.TreeMembership);
        IsNull  (child.Archetype);
        AreEqual("id: 2  (detached)", child.ToString());
        IsTrue(subChild ==  store.GetEntityById(3));
        AreEqual(detached,  child.StoreOwnership);
        IsNull  (child.Archetype);
        IsNull  (child.Store);
        
        var childNode = store.GetEntityNode(2); // child is detached => all fields have their default value
        var entity2   = store.GetEntityById(2);
        IsTrue  (           store.GetEntityById(childNode.Id).IsNull);
        AreEqual(2,         childNode.Id);
        AreEqual(0,         childNode.Pid);
    //  child.TryGetComponent<TreeNode>(out var childTreeNode);
    //  AreEqual(0,         childTreeNode.ChildIds.Length);
    //  AreEqual(0,         childTreeNode.ChildCount);
    //  AreEqual(0,         entity2.Parent.Id);
        AreEqual(NullNode,  childNode.Flags);
        
        // From now: access to components and tree nodes throw a NullReferenceException
        Throws<NullReferenceException> (() => {
            _ = child.Name; // access component
        });
        Throws<NullReferenceException> (() => {
            _ = child.Parent; // access tree node
        });
    }
    
    /// <summary>cover <see cref="EntityStore.DeleteNode"/></summary>
    [Test]
    public static void Test_Test_Entity_Tree_cover_DeleteNode()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        var child4  = store.CreateEntity(4);
        AreEqual(0, root.AddChild(child2));
        AreEqual(1, root.AddChild(child3));
        AreEqual(2, root.AddChild(child4));
        AreEqual(3, root.ChildCount);
        
        AddHandler(store, args => AreEqual("entity: 1 - event > Remove Child[1] = 3", args.ToString()));
        child3.DeleteEntity();
        AreEqual(2, root.ChildCount);
        AreEqual(2, root.ChildEntities[0].Id);
        AreEqual(4, root.ChildEntities[1].Id);
        
        var childIds = root.ChildIds; 
        AreEqual(2, childIds.Length);
        AreEqual(2, childIds[0]);
        AreEqual(4, childIds[1]);
    }
    
    /*
    /// <summary>Cover <see cref="Entity.GetChildNodeByIndex"/></summary>
    [Test]
    public static void Test_Entity_GetChildNodeByIndex()
    {
        var store   = new EntityStore();
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
       
        var child2      = store.CreateEntity(2);
        AreEqual(0,         root.AddChild(child2));
        
        IsTrue(child2 ==    root.ChildEntities[0]);
    } */
    
    /// <summary>Cover <see cref="Entity.GetChildIndex"/></summary>
    [Test]
    public static void Test_Entity_GetChildIndex()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        AreEqual(-1, root.GetChildIndex(entity2));
    }
    
    /// <summary>Cover <see cref="EntityStore.OnChildEntitiesChanged"/></summary>
    [Test]
    public static void Test_EntityStore_ChildEntitiesChanged()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var child2  = store.CreateEntity(2);
        var child3  = store.CreateEntity(3);
        root.AddComponent(new EntityName("root"));
        int eventCount = 0;
        
        Action<ChildEntitiesChanged> handler = args => {
            AreEqual("entity: 1 - event > Add Child[0] = 2", args.ToString());
            eventCount++;
        };
        store.OnChildEntitiesChanged += handler;
        root.AddChild(child2);
        AreEqual(1, eventCount);
        
        store.OnChildEntitiesChanged -= handler;
        root.AddChild(child3);
        AreEqual(1, eventCount); // no event fired
    }
    
    [Test]
    public static void Test_Entity_Tree_ChildEnumerator()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity(1);
        var child   = store.CreateEntity(2);
        root.AddChild(child);
        {
            IEnumerable<Entity> childEntities = root.ChildEntities;
            int count = 0;
            foreach (var _ in childEntities) {
                count++;
            }
            AreEqual(1, count);
        } {
            IEnumerable childEntities = root.ChildEntities;
            int count = 0;
            foreach (var _ in childEntities) {
                count++;
            }
            AreEqual(1, count);
        } {
            ChildEnumerator enumerator = root.ChildEntities.GetEnumerator();
            while (enumerator.MoveNext()) { }
            enumerator.Reset();
            
            int count = 0;
            while (enumerator.MoveNext()) {
                count++;
            }
            enumerator.Dispose();
            AreEqual(1, count);
        }
    }
    
    [Test]
    public static void Test_Entity_id_argument_exceptions()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var e = Throws<ArgumentException>(() => {
            store.CreateEntity(0);
        });
        AreEqual("invalid entity id <= 0. was: 0 (Parameter 'id')", e!.Message);
        
        store.CreateEntity(42);
        e = Throws<ArgumentException>(() => {
            store.CreateEntity(42);
        });
        AreEqual("id already in use in EntityStore. id: 42 (Parameter 'id')", e!.Message);
    }
    
    [Test]
    public static void Test_Entity_SetRoot_assertions()
    {
        {
            var store   = new EntityStore(PidType.RandomPids);
            var e       = Throws<ArgumentNullException>(() => {
                store.SetStoreRoot(default);
            });
            AreEqual("entity", e!.ParamName);
        } {
            var store1  = new EntityStore(PidType.RandomPids);
            var store2  = new EntityStore(PidType.RandomPids);
            var entity  = store1.CreateEntity();
            var e       = Throws<ArgumentException>(() => {
                store2.SetStoreRoot(entity);            
            });
            AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
        } {
            var store   = new EntityStore(PidType.RandomPids);
            var entity1 = store.CreateEntity();
            var entity2 = store.CreateEntity();
            store.SetStoreRoot(entity1);
            var e = Throws<InvalidOperationException>(() => {
                store.SetStoreRoot(entity2);
            });
            AreEqual("EntityStore already has a StoreRoot. StoreRoot id: 1", e!.Message);
        } {
            var store   = new EntityStore(PidType.RandomPids);
            var entity1 = store.CreateEntity();
            var entity2 = store.CreateEntity();
            entity1.AddChild(entity2);
            var e = Throws<InvalidOperationException>(() => {
                store.SetStoreRoot(entity2);
            });
            AreEqual("entity must not have a parent to be StoreRoot. current parent id: 1", e!.Message);
        }
    }
    
    [Test]
    public static void Test_Entity_Tree_InvalidStoreException()
    {
        var store1   = new EntityStore(PidType.RandomPids);
        var store2   = new EntityStore(PidType.RandomPids);
        
        var entity1 = store1.CreateEntity();
        var entity2 = store2.CreateEntity();

        var e = Throws<ArgumentException>(() => {
            entity1.AddChild(entity2);
        });
        AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
        
        e = Throws<ArgumentException>(() => {
            entity1.RemoveChild(entity2);
        });
        AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
        
        e = Throws<ArgumentException>(() => {
            entity1.InsertChild(0, entity2);
        });
        AreEqual("entity is owned by a different store (Parameter 'entity')", e!.Message);
    }

    /// <summary><see cref="EntityStore.GenerateRandomPidForId"/></summary>
    [Test]
    public static void Test_Entity_Tree_RandomPid_Coverage()
    {
        var store   = new EntityStore(PidType.RandomPids);
        store.SetRandomSeed(1);
        store.CreateEntity();
        store.SetRandomSeed(1); // Random generate same pid. use Next() pid
        store.CreateEntity();
    }
    
    [Test]
    public static void Test_Add_Child_Entities_UseRandomPids_Perf()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Root"));
        var sw = new Stopwatch();
        sw.Start();
        long count  = 10; // 10_000_000L ~ #PC: 3.101 ms
        for (long n = 0; n < count; n++) {
            var child = store.CreateEntity();
            root.AddChild(child);
        }
        Console.WriteLine($"CreateEntity(idType.RandomPids) - count: {count}, duration: {sw.ElapsedMilliseconds}");
        AreEqual(count, root.ChildCount);
    }
    
    [Test]
    public static void Test_Add_Child_Entities_UsePidAsId_Perf()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Root"));
        var sw = new Stopwatch();
        sw.Start();
        long count  = 10; // 10_000_000L ~ #PC: 1.215 ms
        for (long n = 0; n < count; n++) {
            var child = store.CreateEntity();
            root.AddChild(child);
        }
        Console.WriteLine($"CreateEntity(PidType.UsePidAsId) - count: {count}, duration: {sw.ElapsedMilliseconds}");
        AreEqual(count, root.ChildCount);
    }
    
    [Test]
    public static void Test_Math_Perf()
    {
        var rand = new Random();
        var count = 10; // 10_000_000 ~ #PC: 39 ms
        for (int n = 0; n < count; n++) {
            rand.Next();
        }
    }
}

}


