using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_Query_ForEach
{
    [Test]
    public static void Test_Query_ForEach_Revision()
    {
        var store   = new EntityStore();
        for (int i = 0; i < 10; i++) {
            var e = store.CreateEntity(new Position(), new Rotation(), new Scale3(), new MyComponent1(), new MyComponent2());
            e.DeleteEntity();
        }
        var entity = store.CreateEntity(new Position(), new Rotation(), new Scale3(), new MyComponent1(), new MyComponent2());
        AreEqual(10, entity.Revision);
        {
            var query = store.Query<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
            query.ForEachEntity((ref Position _, ref Rotation _, ref Scale3 _, ref MyComponent1 _, ref MyComponent2 _, Entity e) => {
                IsTrue(entity == e);    // check id & revision
            });
        }
        {
            var query = store.Query<Position, Rotation, Scale3, MyComponent1>();
            query.ForEachEntity((ref Position _, ref Rotation _, ref Scale3 _, ref MyComponent1 _, Entity e) => {
                IsTrue(entity == e);    // check id & revision
            });
        }
        {
            var query = store.Query<Position, Rotation, Scale3>();
            query.ForEachEntity((ref Position _, ref Rotation _, ref Scale3 _, Entity e) => {
                IsTrue(entity == e);    // check id & revision
            });
        }
        {
            var query = store.Query<Position, Rotation>();
            query.ForEachEntity((ref Position _, ref Rotation _, Entity e) => {
                IsTrue(entity == e);    // check id & revision
            });
        }
        {
            var query = store.Query<Position>();
            query.ForEachEntity((ref Position _, Entity e) => {
                IsTrue(entity == e);    // check id & revision
            });
        }
    }
    
    [Test]
    public static void Test_Query_ForEach_Entity_Perf()
    {
        var count = 5; // 5_000_000
        var store   = new EntityStore();
        for (int n = 1; n <= 1000; n++) {
            store.CreateEntity(new Position());
        }
        var query = store.Query<Position>();
        for (int n = 1; n <= count; n++) {
            query.ForEachEntity(static (ref Position position, Entity entity) => {
                position.x++;
            });
        }
    }
    
    [Test]
    public static void Test_Query_ForEach_Chunks_Perf()
    {
        var count = 5; // 5_000_000
        var store   = new EntityStore();
        for (int n = 1; n <= 1000; n++) {
            store.CreateEntity(new Position());
        }
        var query = store.Query<Position>();
        for (int n = 1; n <= count; n++) {
            foreach (var (positions, entities) in query.Chunks) {
                var chunkSpan = positions.Span;
                for (int i = 0; i < chunkSpan.Length; i++) {
                    chunkSpan[i].x++;                    
                }
            }
        }
    }
}

}

