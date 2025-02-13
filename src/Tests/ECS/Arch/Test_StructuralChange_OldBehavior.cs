using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;


// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_StructuralChange_OldBehavior
{
    [Test]
    public static void Test_StructuralChange_OldBehavior_no_throw()
    {
        var store = new EntityStore();
        store.CreateEntity(new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent4());
        {
            var query = store.Query();
            query.ThrowOnStructuralChange = false;
            foreach (var entity in query.Entities)
            {
                TestNoExceptions(store, entity);
            }
        } {
            var query = store.Query<MyComponent1>();
            query.ThrowOnStructuralChange = false;
            query.ForEachEntity((ref MyComponent1 _, Entity entity) =>
            {
                TestNoExceptions(store, entity);
            });
        }{
            var query = store.Query<MyComponent1, MyComponent2>();
            query.ThrowOnStructuralChange = false;
            query.ForEachEntity((ref MyComponent1 _, ref MyComponent2 _, Entity entity) => 
            {
                TestNoExceptions(store, entity);
            });
        }{
            var query = store.Query<MyComponent1, MyComponent2, MyComponent3>();
            query.ThrowOnStructuralChange = false;
            query.ForEachEntity((ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, Entity entity) => 
            {
                TestNoExceptions(store, entity);
            });
        }{
            var query = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4>();
            query.ThrowOnStructuralChange = false;
            query.ForEachEntity((ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, Entity entity) => 
            {
                TestNoExceptions(store, entity);
            });
        }{
            var query = store.Query<MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>();
            query.ThrowOnStructuralChange = false;
            query.ForEachEntity((ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, ref MyComponent5 _, Entity entity) => 
            {
                TestNoExceptions(store, entity);
            });
        }
    }
    
    private static void TestNoExceptions(EntityStore store, Entity entity)
    {
        entity.AddTag<TestTag>();
        entity.RemoveTag<TestTag>();

        entity.AddComponent<Position>();
        entity.RemoveComponent<Position>();
        
        var buffer = store.GetCommandBuffer();
        buffer.Playback();
        
        var entityBatch = new EntityBatch();
        entityBatch.ApplyTo(entity);
        
        TestMultiAddRemoveNoExceptions(entity);
        
        var converter = EntityConverter.Default;
        var dataEntity = new DataEntity { pid = 1  };
        converter.DataEntityToEntity(dataEntity, store, out _);
    }
    
        
    private static void TestMultiAddRemoveNoExceptions(Entity entity)
    {
        // --- add multiple components
        entity.Add(new Position());
        entity.Add(new Position(), new Scale3());
        entity.Add(new Position(), new Scale3(), new Rotation());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6());
        entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6(), new MyComponent7());
        
        // --- remove multiple components
        entity.Remove<Position>();
        entity.Remove<Position, Scale3>();
        entity.Remove<Position, Scale3, Rotation>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5, MyComponent6>();
        entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5, MyComponent6, MyComponent7>();
    }
    
}

}

