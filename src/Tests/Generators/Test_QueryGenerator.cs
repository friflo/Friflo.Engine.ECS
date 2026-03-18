using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable CheckNamespace
namespace GeneratedCode;


public partial class MyExample
{
    [Query] // This triggers the generator
    void MoveExample(ref Position position) { }
}

public static class Test_QueryGenerator
{
    [Test]
    public static void Test_Entity_new_EntityStore_Perf()
    {
        var tester = new MyExample();
        var store = new EntityStore();
        tester.MoveExampleQuery(store);
    }
}