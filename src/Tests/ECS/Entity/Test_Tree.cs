using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS
{
public struct GlobalPosition : IComponent
{
    public Vector3 value;
    public readonly override string ToString() => value.ToString();
}

public static class Test_Tree
{

    [Test]
    public static void Test_Tree_traverse()
    {
        var store   = new EntityStore();
        var root    = store.CreateEntity(new GlobalPosition());
        var child1  = store.CreateEntity(new GlobalPosition(), new Position(5, 0, 0));
        var child2  = store.CreateEntity(new GlobalPosition(), new Position(0, 3, 0));
        root.AddChild(child1);
        root.AddChild(child2);
        
        TraverseHierarchy(root);
        
        AreEqual(new Vector3(5,0,0), child1.GetComponent<GlobalPosition>().value);
        AreEqual(new Vector3(0,3,0), child2.GetComponent<GlobalPosition>().value);
    }
    
    private static void TraverseHierarchy(Entity parent)
    {
        var parentGlobal = parent.GetComponent<GlobalPosition>().value;
        foreach (var child in parent.ChildEntities) {
            var data = child.Data;
            if (!data.Has<Position>()) {
                continue;
            }
            data.Get<GlobalPosition>().value = parentGlobal + data.Get<Position>().value;
            TraverseHierarchy(child);
        }
    }
}
}