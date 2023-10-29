using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor;

[Component("TestComponent")]
public struct  TestComponent : IComponent
{
    public string name;
}

public struct TestTag : IEntityTag { }


[Behavior("TestBehavior")]
public class TestBehavior : Behavior
{
    public string name;
}