using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
namespace Tests.ECS.GE;


public static class Test_Behavior
{
    private const long Count = 10; // 1_000_000_000L
    
    [Test]
    public static void Test_1_AddComponent() {
        var store   = new GameEntityStore();
        var player  = store.CreateEntity();
        AreEqual("id: 1  []",   player.ToString());
        AreSame(store,          player.Archetype.Store);
        
        // --- add behavior
        var testRef1 = new TestBehavior1 { val1 = 1 };
        IsNull(player.AddBehavior(testRef1));
        NotNull(testRef1.Entity);
        AreSame(testRef1,       player.GetBehavior<TestBehavior1>());
        AreEqual(1,             player.Behaviors.Length);
        AreEqual("id: 1  [*TestBehavior1]", player.ToString());
        AreEqual(1,             player.Behaviors.Length);
        AreSame (testRef1,      player.Behaviors[0]);
        AreEqual(1,             store.EntityBehaviors.Length);
        
        var e = Throws<InvalidOperationException> (() => {
            player.AddBehavior(testRef1);
        });
        AreEqual("behavior already added to an entity. current entity id: 1", e!.Message);
        AreEqual(1,             player.Behaviors.Length);
        
        var testRef2 = new TestBehavior2 { val2 = 2 };
        IsNull (player.AddBehavior(testRef2));
        NotNull (testRef2.Entity);
        
        AreSame (testRef2,      player.GetBehavior<TestBehavior2>());
        AreEqual(2,             player.Behaviors.Length);
        AreEqual("id: 1  [*TestBehavior1, *TestBehavior2]", player.ToString());
        AreEqual(1,             store.EntityBehaviors.Length);
        
        var testRef3 = new TestBehavior2();
        NotNull (player.AddBehavior(testRef3));
        IsNull  (testRef2.Entity);
        NotNull (testRef3.Entity);
        AreSame (testRef3,      player.GetBehavior<TestBehavior2>());
        AreEqual(2,             player.Behaviors.Length);
        AreEqual("id: 1  [*TestBehavior1, *TestBehavior2]", player.ToString());
        
        for (long n = 0; n < Count; n++) {
            _ = player.GetBehavior<TestBehavior1>();
        }
    }
    
    [Test]
    public static void Test_2_RemoveBehavior() {
        var store   = new GameEntityStore();
        var player = store.CreateEntity();
        
        var testRef1 = new TestBehavior1();
        IsFalse(player.TryGetBehavior<TestBehavior1>(out _));
        IsNull(player.RemoveBehavior<TestBehavior1>());
        AreEqual("id: 1  []",               player.ToString());
        AreEqual(0,                         player.Behaviors.Length);
        AreEqual("[*TestBehavior1]",        testRef1.ToString());
        
        player.AddBehavior(testRef1);
        AreEqual(1,                         player.Behaviors.Length);
        AreSame (testRef1, player.GetBehavior<TestBehavior1>());
        IsTrue  (player.TryGetBehavior<TestBehavior1>(out var result));
        AreSame (testRef1, result);
        AreEqual("id: 1  [*TestBehavior1]", player.ToString());
        NotNull (testRef1.Entity);
        IsFalse (player.TryGetBehavior<TestBehavior2>(out _));
        
        NotNull (player.RemoveBehavior<TestBehavior1>());
        AreEqual(0,                         player.Behaviors.Length);
        IsNull  (player.GetBehavior<TestBehavior1>());
        IsFalse (player.TryGetBehavior<TestBehavior1>(out _));
        AreEqual("id: 1  []",               player.ToString());
        IsNull(testRef1.Entity);
        
        IsNull(player.RemoveBehavior<TestBehavior1>());
        AreEqual(0,                         player.Behaviors.Length);
    }
    
    [Test]
    public static void Test_3_RemoveBehavior() {
        var store   = new GameEntityStore();
        var player = store.CreateEntity();
        
        IsNull  (player.AddBehavior(new TestBehavior1 { val1 = 1 }));
        IsNull  (player.AddBehavior(new TestBehavior2 { val2 = 2 }));
        IsNull  (player.AddBehavior(new TestBehavior3 { val3 = 3 }));
        NotNull (player.RemoveBehavior<TestBehavior2>());
        AreEqual(2, player.Behaviors.Length);
        
        NotNull(player.GetBehavior<TestBehavior1>());
        IsNull (player.GetBehavior<TestBehavior2>());
        NotNull(player.GetBehavior<TestBehavior3>());
    }
    
    /// <summary>Cover move last behavior in <see cref="GameEntityStore.RemoveBehavior"/> </summary>
    [Test]
    public static void Test_3_cover_move_last_behavior() {
        var store   = new GameEntityStore();
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        
        IsNull  (entity1.AddBehavior(new TestBehavior1 { val1 = 1 }));
        IsNull  (entity2.AddBehavior(new TestBehavior2 { val2 = 2 }));
        AreEqual(1,                         entity1.Behaviors.Length);
        AreEqual(1,                         entity2.Behaviors.Length);
        AreEqual(2,                         store.EntityBehaviors.Length);
        
        NotNull (entity1.RemoveBehavior<TestBehavior1>());
        AreEqual(0,                         entity1.Behaviors.Length);
        AreEqual(1,                         store.EntityBehaviors.Length);
        NotNull (entity2.RemoveBehavior<TestBehavior2>());
        AreEqual(0,                         entity2.Behaviors.Length);
        AreEqual(0,                         store.EntityBehaviors.Length);
        
        IsNull  (entity1.GetBehavior<TestBehavior1>());
        IsNull  (entity2.GetBehavior<TestBehavior2>());
    }
    
    /// <summary>Cover <see cref="GameEntityUtils.RemoveBehavior"/></summary>
    [Test]
    public static void Test_3_cover_remove_non_added_behavior() {
        var store   = new GameEntityStore();
        var entity  = store.CreateEntity();
        
        IsNull  (entity.AddBehavior(new TestBehavior1 { val1 = 1 }));
        AreEqual(1, entity.Behaviors.Length);
        
        IsNull  (entity.RemoveBehavior<TestBehavior2>());
        AreEqual(1, entity.Behaviors.Length); // remains unchanged
    }
    
    [Test]
    public static void Test_3_InvalidRefComponent() {
        var store   = new GameEntityStore();
        var player  = store.CreateEntity();
        
        var testRef1 = new InvalidRefComponent();
        var e = Throws<InvalidOperationException>(() => {
            player.AddBehavior(testRef1); 
        });
        AreEqual("Missing attribute [Behavior(\"<key>\")] on type: Tests.ECS.InvalidRefComponent", e!.Message);
        AreEqual(0, player.Behaviors.Length);
        
        var behavior = player.GetBehavior<InvalidRefComponent>();
        IsNull  (behavior);
        
        // throws currently no exception
        player.RemoveBehavior<InvalidRefComponent>();
        AreEqual(0, player.Behaviors.Length);
    }
    
    [Test]
    public static void Test_2_Perf() {
        var store   = new GameEntityStore();
        var list = new List<GameEntity>();
        for (long n = 0; n < 10; n++) {
            list.Add(store.CreateEntity());
        }
        IsTrue(list.Count > 0);
    }
    
    [Test]
    public static void Test_GetBehavior_Perf() {
        var store   = new GameEntityStore();
        var player  = store.CreateEntity();
        player.AddBehavior(new TestBehavior1());
        NotNull(player.GetBehavior<TestBehavior1>());
        
        const int count = 10; // 1_000_000_000 ~ 5.398 ms
        for (long n = 0; n < count; n++) {
            player.GetBehavior<TestBehavior1>();
        }
    }
    
    [Test]
    public static void Test_3_Perf_Add_Remove_Component() {
        var store   = new GameEntityStore();
        var player  = store.CreateEntity();
        AreEqual("id: 1  []", player.ToString());
        
        const int count = 10; // 100_000_000 ~ 3.038 ms
        for (long n = 0; n < count; n++) {
            var testRef1 = new TestBehavior1();
            player.AddBehavior(testRef1);
            player.RemoveBehavior<TestBehavior1>();
        }
    }
    
    [Behavior("empty")]
    private class EmptyBehavior : Behavior { }
    
    [Test]
    public static void Test_Empty_Lifecycle_methods() {
        var empty = new EmptyBehavior();
        empty.Start();
        empty.Update();
    }
    
    /* Editor Inspector would look like
    
    Entity              id 0    
    > TestComponent     health 4
    > Position          x 1     y 0     z 0
    > MyComponent1      a 1
         
    */
    [Test]
    public static void Test_3_Simulate_Editor() {
        var store   = new GameEntityStore();
        var entity  = store.CreateEntity();
        
        var test    = new TestComponent();
        entity.AddBehavior(test);                       // component added via editor
        entity.AddComponent(new Position { x = 1 });    // behavior added via editor
        entity.AddComponent(new MyComponent1 { a = 1}); // behavior added via editor
        
        AreEqual(1, entity.Behaviors.Length);
        AreEqual(2, entity.Archetype.ComponentCount);
        AreEqual("id: 1  [*TestComponent, Position, MyComponent1]", entity.ToString());
        AreSame(entity, test.Entity);
        test.Start();
        test.Update();
    }
}




