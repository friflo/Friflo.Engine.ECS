using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Systems {

public static class Test_ChunkEntities
{
    [Test]
    public static void Test_ChunkEntities_Enumerator()
    {
        var store = Test_ScriptSystems.SetupTestStore();
        var root  = store.StoreRoot;
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = child.Archetype.CreateEntity();
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        
        var query = store.Query<Position>();
        int chunkCount  = 0;
        foreach (var (position, entities) in query.Chunks)
        {
            switch (chunkCount++) { 
                case 0:
                    Mem.AreEqual("Entity[1]    Archetype: [EntityName, Position, Rotation, Scale3, Transform, TreeNode, MyComponent1]  entities: 1",   entities.ToString());
                    Mem.AreEqual(1,             entities.Length);
                    var e = Assert.Throws<IndexOutOfRangeException>(() => {
                        _ = entities.EntityAt(1);
                    });
                    Mem.AreEqual("Index was outside the bounds of the array.", e!.Message);
                    
                    e = Assert.Throws<IndexOutOfRangeException>(() => {
                        _ = entities[1];
                    });
                    Mem.AreEqual("Index was outside the bounds of the array.", e!.Message);
                    break;
                case 1:
                    Mem.AreEqual(999,           entities.Length);
                    break;
                default:
                    throw new InvalidOperationException("unexpected");
            }
            {
                int count = 0;
                foreach (var entity in entities) {
                    Mem.AreEqual(entity.Id, position[count].x);
                    count++;
                }
                Mem.AreEqual(entities.Length, count);
            } {
                IEnumerable<Entity> enumerable = entities;
                var enumerator = enumerable.GetEnumerator();
                int count = 0;
                while (enumerator.MoveNext()) {
                    count++;
                }
                Mem.AreEqual(entities.Length, count);
                enumerator.Reset();
                count = 0;
                foreach (var _ in enumerable) {
                    count++;
                }
                enumerator.Dispose();
                Mem.AreEqual(entities.Length, count);
            } {
                IEnumerable childEntities = entities;
                int count = 0;
                foreach (var _ in childEntities) {
                    count++;
                }
                Mem.AreEqual(entities.Length, count);
            }
        }
        Mem.AreEqual(2, chunkCount);
    }
}

}

