using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Internal.ECS;

public static class Test_EntityList
{
    [Test]
    public static void Test_EntityList_AddTreeEntities()
    {
        var count       = 10;   // 10_000_000 ~ #PC: 4304 ms
        var entityCount = 100;
        
        var store   = new EntityStore();
        var root    = store.CreateEntity();
        var arch    = store.GetArchetype(ComponentTypes.Get<Rotation, Position>());
        
        for (int n = 1; n < entityCount; n++) {
            root.AddChild(arch.CreateEntity());
        }
        var list = new EntityList(store);
        
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            list.Clear();
            EntityUtils.AddTreeEntities(root, list);
        }
        Console.WriteLine($"AddTreeEntities - duration: {sw.ElapsedMilliseconds} ms");
        AreEqual(entityCount, list.count);
    }
}