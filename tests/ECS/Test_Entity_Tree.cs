using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Friflo.Fliox.Engine.ECS.StoreOwnership;
using static Friflo.Fliox.Engine.ECS.TreeMembership;
using static Friflo.Fliox.Engine.ECS.NodeFlags;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Entity_Tree
{
    [Test]
    public static void Test_CreateEntity_UseRandomPids() {
        var store   = new EntityStore();
        store.SetRandomSeed(0);
        var entity  = store.CreateEntity();
        AreEqual(1,             entity.Id);
        AreEqual(1559595546,    store.Nodes[entity.Id].Pid);
        AreEqual(1,             store.PidToId(1559595546));
        AreEqual(1,             store.GetNodeByPid(1559595546).Id);
    }
    
    [Test]
    public static void Test_CreateEntity_UsePidAsId() {
        var store   = new EntityStore(100, PidType.UsePidAsId);
        var entity  = store.CreateEntity();
        AreEqual(1,     entity.Id);
        AreEqual(1,     store.Nodes[entity.Id].Pid);
        AreEqual(1,     store.PidToId(1));
        AreEqual(1,     store.GetNodeByPid(1).Pid);
    }
    
    [Test]
    public static void Test_AddChild() {
        var store   = new EntityStore();
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
       
        IsNull  (root.Parent);
        AreEqual(0,         root.ChildNodes.Ids.Length);
        AreEqual(0,         root.ChildCount);
        AreEqual(attached,  root.StoreOwnership);
        foreach (ref var _ in root.ChildNodes) {
            Fail("expect empty child nodes");
        }
        
        // -- add child
        var child = store.CreateEntity(4);
        AreEqual(4,         child.Id);
        child.AddComponent(new EntityName("child"));
        root.AddChild(child);
        
        IsNull  (child.Root);
        AreSame (root,      child.Parent);
        AreEqual(attached,  child.StoreOwnership);
        var childNodes =    root.ChildNodes;
        AreEqual(1,         childNodes.Ids.Length);
        AreEqual(4,         childNodes.Ids[0]);
        AreEqual(1,         root.ChildCount);
        AreSame (child,     root.GetChild(0));
        int count = 0;
        foreach (ref var node in root.ChildNodes) {
            count++;
            AreSame(child, node.Entity);
        }
        AreEqual(1,         count);
        
        // -- add same child again
        root.AddChild(child);
        AreEqual(1,                                 childNodes.Ids.Length);
        var rootNode = store.Nodes[1];
        AreEqual(1,                                 rootNode.ChildCount);
        AreEqual(1,                                 rootNode.ChildIds.Length);
        AreEqual("id: 1  \"root\"  ChildCount: 1  flags: Created",  rootNode.ToString());
        AreSame (child,                             childNodes[0]);
        
        // --- copy child GameEntity's to array
        var array = new GameEntity[childNodes.Length];
        childNodes.ToArray(array);
        AreSame(child, array[0]);
        
#pragma warning disable CS0618 // Type or member is obsolete
        AreEqual(1,                                 childNodes.Entities_.Length);
#pragma warning restore CS0618 // Type or member is obsolete
    }
    
    [Test]
    public static void Test_RemoveChild() {
        var store   = new EntityStore();
        var root    = store.CreateEntity(1);
        var child   = store.CreateEntity(2);
        AreEqual(floating,  root.TreeMembership);
        AreEqual(floating,  child.TreeMembership);
        
        root.AddChild(child);
        IsNull  (child.Root);
        
        store.SetRoot(root);
        NotNull (root.Root);
        NotNull (child.Root);
        AreEqual(treeNode,  root.TreeMembership);
        AreEqual(treeNode,  child.TreeMembership);
        
        // --- remove child
        root.RemoveChild(child);
        IsNull  (child.Root);
        AreEqual(floating,  child.TreeMembership);
        AreEqual(0,         root.ChildCount);
        IsNull  (child.Parent);
        
        // --- remove same child again
        root.RemoveChild(child);
        
        AreEqual(0,         root.ChildCount);
        IsNull  (child.Parent);
        AreEqual(2,         store.EntityCount);
    }
    
    [Test]
    public static void Test_SetRoot() {
        var store   = new EntityStore();
        IsNull (store.Root);
        
        var root    = store.CreateEntity(1);
        IsNull (root.Root);
        IsNull (root.Parent);
        
        store.SetRoot(root);
        AreSame(root,       store.Root);
        AreSame(root,       root.Root);
        IsNull (root.Parent);
        
        var child   = store.CreateEntity(2);
        root.AddChild(child);
        AreSame(root,       child.Root);
        AreEqual(treeNode,  child.TreeMembership);
        AreEqual(2,         store.EntityCount);
        var nodes = store.Nodes;
        AreEqual("id: 0",                               nodes[0].ToString());
        AreEqual("id: 2  []  flags: TreeNode | Created",nodes[2].ToString());
        
        AreEqual(NullNode,                              nodes[0].Flags);
        AreEqual(TreeNode | Created,                    nodes[2].Flags);
    }
    
    [Test]
    public static void Test_move_with_AddChild() {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var child   = store.CreateEntity(3);
        entity1.AddComponent(new EntityName("entity-1"));
        entity2.AddComponent(new EntityName("entity-2"));
        child.AddComponent  (new EntityName("child"));
        
        entity1.AddChild(child);
        AreEqual(1,     entity1.ChildCount);
        
        // --- move child from entity1 -> entity2
        entity2.AddChild(child);
        AreEqual(0,     entity1.ChildCount);
        AreEqual(1,     entity2.ChildCount);
        AreSame(child,  entity2.GetChild(0));
    }
    
    [Test]
    public static void Test_DeleteEntity() {
        var store   = new EntityStore();
        var root    = store.CreateEntity(1);
        root.AddComponent(new EntityName("root"));
        store.SetRoot(root);
        var child   = store.CreateEntity(2);
        AreEqual(attached,  child.StoreOwnership);
        child.AddComponent(new EntityName("child"));
        root.AddChild(child);
        var subChild= store.CreateEntity(3);
        subChild.AddComponent(new EntityName("subChild"));
        child.AddChild(subChild);
        
        AreEqual(3,         store.EntityCount);
        AreSame (root,      child.Root);
        AreSame (root,      subChild.Root);
        
        child.DeleteEntity();
        AreEqual(2,         store.EntityCount);
        AreEqual(0,         root.ChildCount);
        IsNull  (subChild.Root);        // subChild is floating
        IsNull  (child.Archetype);
        AreEqual("id: 2  (detached)", child.ToString());
        AreSame (subChild,   store.Nodes[3].Entity);
        AreEqual(detached,  child.StoreOwnership);
        
        var childNode = store.Nodes[2]; // child is detached => all fields have their default value
        IsNull  (           childNode.Entity);
        AreEqual(2,         childNode.Id);
        AreEqual(0,         childNode.Pid);
        AreEqual(0,         childNode.ChildIds.Length);
        AreEqual(0,         childNode.ChildCount);
        AreEqual(0,         childNode.ParentId);
        AreEqual(NullNode,  childNode.Flags);
        
        // From now: access to struct components and tree nodes throw a NullReferenceException
        Throws<NullReferenceException> (() => {
            _ = child.Name; // access struct component
        });
        Throws<NullReferenceException> (() => {
            _ = child.Root; // access tree node
        });
    }
    
    [Test]
    public static void Test_Add_Child_Entities_UseRandomPids_Perf() {
        var store   = new EntityStore();
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Root"));
        long count  = 10; // 10_000_000L ~ 4.009 ms
        for (long n = 0; n < count; n++) {
            var child = store.CreateEntity();
            root.AddChild(child);
        }
        AreEqual(count, root.ChildCount);
    }
    
    [Test]
    public static void Test_Add_Child_Entities_UsePidAsId_Perf() {
        var store   = new EntityStore(100, PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("Root"));
        long count  = 10; // 10_000_000L ~ 2.014 ms
        for (long n = 0; n < count; n++) {
            var child = store.CreateEntity();
            root.AddChild(child);
        }
        AreEqual(count, root.ChildCount);
    }
    
    [Test]
    public static void Test_Math_Perf() {
        var rand = new Random();
        var count = 10; // 10_000_000 ~  39 ms
        for (int n = 0; n < count; n++) {
            rand.Next();
        }
    }
}

