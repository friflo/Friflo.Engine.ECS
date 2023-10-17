using System;
using Friflo.Fliox.Engine.ECS;
using static NUnit.Framework.Assert;

#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value

namespace Tests.ECS;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class CodeCoverageTestAttribute : Attribute { }


// ------------------------------------------------ struct components
[CodeCoverageTest]
[StructComponent("my1")]
public struct MyComponent1 : IStructComponent { public int a; }

[StructComponent("my2")]
public struct MyComponent2 : IStructComponent { public int b; }

/// <summary>Example shows an extension class to enable component access using less code.</summary>
public static class EntityExtensions
{
    public static ref MyComponent1 MyComponent1(this GameEntity entity) => ref entity.Component<MyComponent1>();
    public static ref MyComponent2 MyComponent2(this GameEntity entity) => ref entity.Component<MyComponent2>();
}

// test missing [StructComponent()] attribute
struct MyInvalidComponent  : IStructComponent { public int b; }


// ------------------------------------------------ tags
public struct TestTag  : IEntityTag { }

public struct TestTag2 : IEntityTag { }

public struct TestTag3 : IEntityTag { }


// ------------------------------------------------ class components
[CodeCoverageTest]
[ClassComponent("testRef1")]
class TestRefComponent1 : ClassComponent {
    public int  val1;
}

[ClassComponent("testRef2")]
class TestRefComponent2 : ClassComponent {
    public int  val2;
}

[ClassComponent("testRef3")]
class TestRefComponent3 : ClassComponent {
    public int  val3;
}

// test missing [ClassComponent()] attribute
class InvalidRefComponent : ClassComponent { }


[ClassComponent("test")]
class TestComponent : ClassComponent
{
    private int                      health;     // is serialized
    
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
