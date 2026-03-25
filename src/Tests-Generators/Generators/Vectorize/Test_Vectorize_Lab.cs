using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Examples;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Vectorize;


public static class Test_Vectorize_Lab
{
    // [Query]
    [Vectorize]
    public static void MoveVectorized(ref Position position, in Velocity velocity, float deltaTime) {
        position.value += velocity.value * deltaTime;
    }
    
#region template for code generation
    public static ArchetypeQuery MoveVectorizedQuery(EntityStore store, float deltaTime)
    {
        var query = MoveVectorized_GetQuery(store);
        foreach (var chunk in query.Chunks)
        {
            var entities = chunk.Entities;
            var positionSpan = chunk.Chunk1.Span;
            var velocitySpan = chunk.Chunk2.Span;
            for (int n = 0; n < entities.Length; n++) {
                MoveVectorized(ref positionSpan[n], velocitySpan[n], deltaTime);
            }
        }
        return query;
    }
    
    private static readonly int MoveVectorized_Slot = EntityStore.UserDataNewSlot();
    
    private static ArchetypeQuery<Position, Velocity> MoveVectorized_GetQuery(EntityStore store)
    {
        var query = (ArchetypeQuery<Position, Velocity>)EntityStore.UserDataGet(store, MoveVectorized_Slot);
        if (query != null) {
            return query;
        }
        query = store.Query<Position, Velocity>();
        
        EntityStore.UserDataSet(store, MoveVectorized_Slot, query);
        return query;
    }
    #endregion  
    
    [Test]
    public static void Test_Vectorize_Lab_Run()
    {
        var store = new EntityStore();
        for (int n = 0; n < 1000; n++) {
            store.CreateEntity(new Position { x = n, y = 2, z = 3 }, new Velocity() { value = new Vector3(1,0,0 )});
        }
        MoveVectorizedQuery(store, 0.1f);
    }
}

