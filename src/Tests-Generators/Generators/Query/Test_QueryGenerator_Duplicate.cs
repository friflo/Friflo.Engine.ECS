using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Duplicate;


/// Test using same class / method name as in <see cref="Tests.Generators.MyExample.MoveExample"/>
/// Goal: Ensure the source generator uses the namespace in the filename. 
public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position, Entity entity) {
        AreEqual(1, entity.Id); 
        position.x = 1;
    }
}

public partial class MyExample<T>
{
    [Query]
    void MoveExample(ref Position position, T value) {
        position.x = 1;
    }
}

public static class Test_QueryGeneratorDuplicate
{
    [Test]
    public static void Test_QueryGeneratorDuplicate_generate()
    {
        var tester = new MyExample();
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1(), new MyComponent2());

        var query = tester.MoveExampleQuery(store);
        AreEqual(1, entity.GetComponent<Position>().x);
        AreEqual(1, query.Count);
    }
    
    [Test]
    public static void Test_QueryGeneratorDuplicate_generic()
    {
        var tester = new MyExample<int>();
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1(), new MyComponent2());

        var query = tester.MoveExampleQuery(store, 123);
        AreEqual(1, entity.GetComponent<Position>().x);
        AreEqual(1, query.Count);
    }
}