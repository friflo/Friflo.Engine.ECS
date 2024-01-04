using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.System;

public static class Test_ChunkEntities
{
    [Test]
    public static void Test_ChunkEntities_Enumerator()
    {
        var store = Test_Systems.SetupTestStore();
        var root  = store.StoreRoot;
        
        var child = store.CreateEntity();
        root.AddChild(child);
        child.AddComponent(new Position(2, 0, 0));
        for (int n = 3; n <= 1000; n++) {
            child = store.CreateEntity(child.Archetype);
            child.Position = new Position(n, 0, 0);
            root.AddChild(child);
        }
        
        var query = store.Query<Position>();
        int chunkCount  = 0;
        foreach (var (position, entities) in query.Chunks)
        {
            switch (chunkCount++) { 
                case 0:
                    Mem.AreEqual("Position[1]",   position.ToString());
                    Mem.AreEqual("Archetype[1]: [EntityName, Position, Rotation, Transform, Scale3, MyComponent1]  Count: 1",   entities.ToString());
                    Mem.AreEqual(1,             entities.Length);
                    var e = Assert.Throws<IndexOutOfRangeException>(() => {
                        _ = entities.EntityAt(1);
                    });
                    Mem.AreEqual("Index was outside the bounds of the array.", e!.Message);
                    
                    e = Assert.Throws<IndexOutOfRangeException>(() => {
                        _ = entities.IdAt(1);
                    });
                    Mem.AreEqual("Index was outside the bounds of the array.", e!.Message);
                    break;
                case 1:
                    Mem.AreEqual(512,           entities.Length);
                    break;
                case 2:
                    Mem.AreEqual(487,           entities.Length);
                    break;
                case 3:
                    throw new InvalidOperationException("unexpected");
            }
            {
                int count = 0;
                foreach (var entity in entities) {
                    Mem.AreEqual(entity.Id, position.Values[count].x);
                    count++;
                }
                Mem.AreEqual(entities.Length, count);
            } {
                IEnumerable<Entity> enumerable = entities;
                int count = 0;
                foreach (var _ in enumerable) {
                    count++;
                }
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
        Mem.AreEqual(3, chunkCount);
    }
}

