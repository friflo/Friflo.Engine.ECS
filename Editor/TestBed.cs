using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor;

[Component("TestComponent")]
public struct  TestComponent : IComponent
{
    public string name;
}

public struct TestTag : IEntityTag { }


[Script("TestScript")]
public class TestScript : Script
{
    public string name;
}