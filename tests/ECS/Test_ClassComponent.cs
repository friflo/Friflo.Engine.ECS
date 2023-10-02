using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.ECS;

[ClassComponent("testRef1")]
class TestRefComponent1 : ClassComponent {
    public int  val1;
}

[ClassComponent("testRef2")]
class TestRefComponent2 : ClassComponent {
    public int  val2;
}

// test missing [ClassComponent()] attribute
class InvalidRefComponent : ClassComponent { }

public static class Test_ClassComponent
{
    private const long Count = 10; // 1_000_000_000L
    
    [Test]
    public static void Test_1_AddComponent() {
        var store   = new EntityStore();
        var player  = store.CreateEntity();
        AreEqual("id: 1  []",   player.ToString());
        AreSame(store,          player.Archetype.Store);
        
        // --- add class component
        var testRef1 = new TestRefComponent1 { val1 = 1 };
        IsNull(player.AddClassComponent(testRef1));
        NotNull(testRef1.Entity);
        AreSame(testRef1,       player.GetClassComponent<TestRefComponent1>());
        AreEqual(1,             player.ComponentCount);
        AreEqual("id: 1  [*TestRefComponent1]", player.ToString());
        AreEqual(1,             player.ClassComponents.Length);
        AreSame (testRef1,      player.ClassComponents[0]);
        
        var e = Throws<InvalidOperationException> (() => {
            player.AddClassComponent(testRef1);
        });
        AreEqual("component already added to an entity", e!.Message);
        AreEqual(1,             player.ComponentCount);
        
        var testRef2 = new TestRefComponent2 { val2 = 2 };
        IsNull (player.AddClassComponent(testRef2));
        NotNull (testRef2.Entity);
        
        AreSame (testRef2,      player.GetClassComponent<TestRefComponent2>());
        AreEqual(2,             player.ComponentCount);
        AreEqual("id: 1  [*TestRefComponent1, *TestRefComponent2]", player.ToString());
        
        var testRef3 = new TestRefComponent2();
        NotNull (player.AddClassComponent(testRef3));
        IsNull  (testRef2.Entity);
        NotNull (testRef3.Entity);
        AreSame (testRef3,      player.GetClassComponent<TestRefComponent2>());
        AreEqual(2,             player.ComponentCount);
        AreEqual("id: 1  [*TestRefComponent1, *TestRefComponent2]", player.ToString());
        
        IsTrue(ClassUtils.RegisteredTypes.ContainsKey(typeof(TestRefComponent1)));
        
#pragma warning disable CS0618 // Type or member is obsolete
        AreEqual(2,             player.Components_.Length);
#pragma warning restore CS0618 // Type or member is obsolete
        
        for (long n = 0; n < Count; n++) {
            _ = player.GetClassComponent<TestRefComponent1>();
        }
    }
    
    [Test]
    public static void Test_2_RemoveComponent() {
        var store   = new EntityStore();
        var player = store.CreateEntity();
        
        var testRef1 = new TestRefComponent1();
        IsFalse(player.TryGetClassComponent<TestRefComponent1>(out _));
        IsNull(player.RemoveClassComponent<TestRefComponent1>());
        AreEqual("id: 1  []",                   player.ToString());
        AreEqual("[*TestRefComponent1]",        testRef1.ToString());
        
        player.AddClassComponent(testRef1);
        AreSame(testRef1, player.GetClassComponent<TestRefComponent1>());
        IsTrue(player.TryGetClassComponent<TestRefComponent1>(out var result));
        AreSame(testRef1, result);
        AreEqual("id: 1  [*TestRefComponent1]", player.ToString());
        NotNull(testRef1.Entity);
        
        NotNull(player.RemoveClassComponent<TestRefComponent1>());
        IsNull(player.GetClassComponent<TestRefComponent1>());
        IsFalse(player.TryGetClassComponent<TestRefComponent1>(out _));
        AreEqual("id: 1  []",                   player.ToString());
        IsNull(testRef1.Entity);
        
        IsNull(player.RemoveClassComponent<TestRefComponent1>());
    }
    
    [Test]
    public static void Test_3_InvalidRefComponent() {
        var store   = new EntityStore();
        var player  = store.CreateEntity();
        
        var testRef1 = new InvalidRefComponent();
        var e = Throws<InvalidOperationException>(() => {
            player.AddClassComponent(testRef1); 
        });
        AreEqual("Missing attribute [ClassComponent(\"<key>\")] on type: Tests.ECS.InvalidRefComponent", e!.Message);
        AreEqual(0, player.ComponentCount);
        
        var component = player.GetClassComponent<InvalidRefComponent>();
        IsNull(component);
        
        // throws currently no exception
        player.RemoveClassComponent<InvalidRefComponent>();
    }
    
    [Test]
    public static void Test_2_Perf() {
        var store   = new EntityStore();
        var list = new List<GameEntity>();
        for (long n = 0; n < 10; n++) {
            list.Add(store.CreateEntity());
        }
        IsTrue(list.Count > 0);
    }
    
    [Test]
    public static void Test_GetClassComponent_Perf() {
        var store   = new EntityStore();
        var player  = store.CreateEntity();
        player.AddClassComponent(new TestRefComponent1());
        NotNull(player.GetClassComponent<TestRefComponent1>());
        
        const int count = 10; // 1_000_000_000 ~ 5.730 ms
        for (long n = 0; n < count; n++) {
            player.GetClassComponent<TestRefComponent1>();
        }
    }
    
    [Test]
    public static void Test_3_Perf_Add_Remove_Component() {
        var store   = new EntityStore();
        var player  = store.CreateEntity();
        AreEqual("id: 1  []", player.ToString());
        
        const int count = 10; // 10_000_000 ~ 640 ms
        for (long n = 0; n < count; n++) {
            var testRef1 = new TestRefComponent1();
            player.AddClassComponent(testRef1);
            player.RemoveClassComponent<TestRefComponent1>();
        }
    }
    
    
    /* Editor Inspector would look like
    
    Entity              id 0    
    > TestComponent     health 4
    > Position          x 1     y 0     z 0
    > MyComponent1      a 1
         
    */
    [Test]
    public static void Test_3_Simulate_Editor() {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        var test    = new TestComponent();
        entity.AddClassComponent(test);                 // struct component added via editor
        entity.AddComponent(new Position { x = 1 });    // class  component added via editor
        entity.AddComponent(new MyComponent1 { a = 1}); // class  component added via editor
        
        AreEqual(3, entity.ComponentCount);
        AreEqual("id: 1  [*TestComponent, Position, MyComponent1]", entity.ToString());
        AreSame(entity, test.Entity);
        test.Start();
        test.Update();
        var position = entity.GetComponent<Position>();
        for (long n = 0; n < Count; n++) {
            _ = position.Value;
        }
    }
}

[ClassComponent("test")]
class TestComponent : ClassComponent
{
    public int                      health;     // is serialized
    
    private Component<MyComponent1> myComponent;
    private Component<Position>     position;
        
    public override void Start() {
        myComponent = Entity.GetComponent<MyComponent1>();
        position    = Entity.GetComponent<Position>();
    }
    
    public override  void Update() {
        health = 4;
        myComponent.Value.a = 5 + health;
        position.Value.x += 1;
        AreEqual(2f, position.Value.x);
    }
}
