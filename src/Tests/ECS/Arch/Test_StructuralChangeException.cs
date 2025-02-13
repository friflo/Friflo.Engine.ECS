using Friflo.Engine.ECS;
using NUnit.Framework;


// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_StructuralChangeException
{
    [Test]
    public static void Test_StructuralChangeException_Entities()
    {
        var store = new EntityStore();
        store.CreateEntity(new MyComponent1(), new MyComponent2(), new MyComponent3(), new MyComponent4(), new MyComponent4());
        foreach (var entity in store.Entities)
        {
            TestExceptions(entity);
        }
    }
    
    private static void TestExceptions(Entity entity)
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

