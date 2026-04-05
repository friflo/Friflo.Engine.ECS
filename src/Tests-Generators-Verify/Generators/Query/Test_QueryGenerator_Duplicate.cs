// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;


// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Query.Duplicate;


/// Test using same class / method name as in <see cref="Tests.Generators.MyExample.MoveExample"/>
/// Goal: Ensure the source generator uses the namespace in the filename. 
public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position, Entity entity) {
        Assert.That(entity.Id, Is.EqualTo(1));
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
        Assert.That(entity.GetComponent<Position>().x, Is.EqualTo(1));
        Assert.That(query.Count, Is.EqualTo(1));
    }
    
    [Test]
    public static void Test_QueryGeneratorDuplicate_generic()
    {
        var tester = new MyExample<int>();
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position(), new MyComponent1(), new MyComponent2());

        var query = tester.MoveExampleQuery(store, 123);
        Assert.That(entity.GetComponent<Position>().x, Is.EqualTo(1));
        Assert.That(query.Count, Is.EqualTo(1));
    }
}