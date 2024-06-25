using Friflo.Engine.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {


public static partial class Test_Index
{
    private static void QueryArg5 (IndexContext cx)
    {
        var store = cx.store;
        var query1  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3>().        HasValue<IndexedName,   string>("find-me");
        var query2  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3>().        HasValue<IndexedInt,    int>   (123);
        var query3  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3>().        HasValue<IndexedName,   string>("find-me").
                                                                              HasValue<IndexedInt,    int>   (123);
        var query5  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3>().        HasValue<AttackComponent, Entity>(cx.target);
        cx.query1 = query1;
        cx.query2 = query2;
        cx.query3 = query3;
        {
            int count = 0;
            query1.ForEachEntity((ref Position _, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(11, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                }
                AreEqual("find-me", entity.GetComponent<IndexedName>().name);
            });
            AreEqual(2, count);
        } { 
            int count = 0;
            query2.ForEachEntity((ref Position _, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(12, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                }
                AreEqual(123, entity.GetComponent<IndexedInt>().value);
            });
            AreEqual(2, count);
        } { 
            var count = 0;
            query3.ForEachEntity((ref Position _, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(11, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                    case 2: AreEqual(12, entity.Id); break;
                }
            });
            AreEqual(3, count);
        } {
            var count = 0;
            query5.ForEachEntity((ref Position _, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, Entity entity) => {
                count++;
                AreEqual(13,        entity.Id);
                AreEqual(cx.target, entity.GetComponent<AttackComponent>().target);
            });
            AreEqual(1, count);
        }
    }
}

}
