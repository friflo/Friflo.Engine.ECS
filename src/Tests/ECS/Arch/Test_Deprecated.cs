using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

#pragma warning disable CS0618 // Type or member is obsolete
public static class Test_Deprecated
{
    [Test]
    public static void Test_Deprecated_properties()
    {
        var store = new EntityStore(PidType.RandomPids);
        
        AreEqual(0, store.EntityCount);     // replaced by Count
        
        var query = store.Query();
        AreEqual(0, query.EntityCount);     // replaced by Count
        
        var archetype = store.GetArchetype(default);
        AreEqual(0, archetype.EntityCount); // replaced by Count
    }
    
    
    [Test]
    public static void Test_Deprecated_QueryChunk_EntityCount()
    {
        var store = new EntityStore(PidType.RandomPids);
        _ = store.Query<Position>()                                                 .Chunks.EntityCount;
        _ = store.Query<Position, Rotation>()                                       .Chunks.EntityCount;
        _ = store.Query<Position, Rotation, Scale3>()                               .Chunks.EntityCount;
        _ = store.Query<Position, Rotation, Scale3, MyComponent1>()                 .Chunks.EntityCount;
        _ = store.Query<Position, Rotation, Scale3, MyComponent1, MyComponent2>()   .Chunks.EntityCount;
    }
    
    [Test]
    public static void Test_Deprecated_TreeNode_ChildIds()
    {
        var node = new TreeNode();
        var e = Throws<InvalidOperationException>(() => {
            _ = node.ChildIds;
        });
        AreEqual("ChildIds is obsolete. Use GetChildIds()", e!.Message);
    }
}

}