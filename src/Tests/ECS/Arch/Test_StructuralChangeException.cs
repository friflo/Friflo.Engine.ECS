using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using NUnit.Framework;


// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_StructuralChangeException
{
    [Test]
    public static void Test_StructuralChangeException_Message()
    {
        var store = new EntityStore();
        store.CreateEntity();
        foreach (var entity in store.Entities)
        {
            var e = Assert.Throws<StructuralChangeException>(() => {
                entity.AddTag<TestTag>();
            });
            Assert.AreEqual("within query loop. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/query#structuralchangeexception", e!.Message);
        }
    }
    
    [Test]
    public static void Test_StructuralChangeException_Entities()
    {
        var store = new EntityStore();
        store.CreateEntity(new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent4());
        foreach (var entity in store.Entities)
        {
            TestExceptions(store, entity);
        }
    }
    
    private static void TestExceptions(EntityStore store, Entity entity)
    {
        Assert.Throws<StructuralChangeException>(() => {
            entity.AddTag<TestTag>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.RemoveTag<TestTag>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.AddComponent<Position>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.RemoveComponent<Position>();
        });
        
        var target = store.CreateEntity();
        Assert.Throws<StructuralChangeException>(() => {
            entity.CopyEntity(target);
        });
        
        var buffer = store.GetCommandBuffer();
        Assert.Throws<StructuralChangeException>(() => {
            buffer.Playback();
        });
        
        var entityBatch = new EntityBatch();
        Assert.Throws<StructuralChangeException>(() => {
            entityBatch.ApplyTo(entity);
        });
        
        TestMultiAddRemoveExceptions(entity);
        
        var converter = EntityConverter.Default;
        var dataEntity = new DataEntity { pid = 1  };
        Assert.Throws<StructuralChangeException>(() => {
            converter.DataEntityToEntity(dataEntity, store, out _);
        });
    }
    
        
    private static void TestMultiAddRemoveExceptions(Entity entity)
    {
        // --- add multiple components
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6());
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Add(new Position(), new Scale3(), new Rotation(), new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent5(), new MyComponent6(), new MyComponent7());
        });
        
        // --- remove multiple components
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5, MyComponent6>();
        });
        Assert.Throws<StructuralChangeException>(() => {
            entity.Remove<Position, Scale3, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4, MyComponent5, MyComponent6, MyComponent7>();
        });
    }
    
}

}

