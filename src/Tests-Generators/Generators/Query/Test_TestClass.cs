

using Friflo.Engine.ECS;
using Tests.ECS;

// Test using class in global namespace
partial class TestClass
{
    [Query]
    [AllComponents<MyComponent1>]
    [AnyComponents<MyComponent2>]
    [WithoutAllComponents<MyComponent3>]
    [WithoutAnyComponents<MyComponent4>]
    [AllTags<TestTag>]
    [AnyTags<TestTag2>]
    [WithoutAllTags<TestTag3>]
    [WithoutAnyTags<TestTag4>]
    private static void MovePosition(ref Position position, float deltaTime) {
        position.x += deltaTime;
    }
}