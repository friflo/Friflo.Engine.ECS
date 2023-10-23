using System;
using Friflo.Fliox.Engine.ECS;
using static NUnit.Framework.Assert;

#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value

namespace Tests.ECS;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class CodeCoverageTestAttribute : Attribute { }

// ------------------------------------------------ struct components
[CodeCoverageTest]
[Component("my1")]
public struct MyComponent1 : IComponent { public int a; }

[Component("my2")]
public struct MyComponent2 : IComponent { public int b; }


[CodeCoverageTest]
[Component("byte")]
public struct ByteComponent : IComponent { public byte b; }

/// <summary>Example shows an extension class to enable component access using less code.</summary>
public static class EntityExtensions
{
    public static ref MyComponent1 MyComponent1(this GameEntity entity) => ref entity.GetComponent<MyComponent1>();
    public static ref MyComponent2 MyComponent2(this GameEntity entity) => ref entity.GetComponent<MyComponent2>();
}

// test missing [StructComponent()] attribute
struct MyInvalidComponent : IComponent { public int b; }


// ------------------------------------------------ tags
public struct TestTag  : IEntityTag { }

public struct TestTag2 : IEntityTag { }

public struct TestTag3 : IEntityTag { }


// ------------------------------------------------ class components
[CodeCoverageTest]
[Behavior("testRef1")]
class TestBehavior1 : Behavior {
    public int  val1;
}

[Behavior("testRef2")]
class TestBehavior2 : Behavior {
    public int  val2;
}

[Behavior("testRef3")]
class TestBehavior3 : Behavior {
    public int  val3;
}

// test missing [Behavior()] attribute
class InvalidRefComponent : Behavior { }


[Behavior("test")]
class TestComponent : Behavior
{
    private int                      health;     // is serialized
    
    public override  void Update() {
        health = 4;
        Entity.GetComponent<MyComponent1>().a = 5 + health;
        Entity.Position.x += 1;
        AreEqual(2f, Entity.Position.x);
    }
}
