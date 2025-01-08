﻿using Friflo.Engine.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index.Query {

public static partial class Test_Index_Query
{
    private static void QueryArg6 (IndexContext cx)
    {
        var store = cx.store;
        var query1  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>().        HasValue<IndexedName,   string>("find-me");
        var query2  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>().        HasValue<IndexedInt,    int>   (123);
        var query3  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>().        HasValue<IndexedName,   string>("find-me").
                                                                              HasValue<IndexedInt,    int>   (123);
        var query4  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>().        HasValue<AttackComponent, Entity>(cx.target);
        var query5  = store.Query<Position, Rotation, MyComponent1, MyComponent2, MyComponent3, MyComponent4>().        ValueInRange<IndexedInt, int>(100, 1000);
        cx.query1 = query1;
        cx.query2 = query2;
        cx.query3 = query3;
        cx.query4 = query4;
        cx.query5 = query5;
        {
            int count = 0;
            query1.ForEachEntity((ref Position pos, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(11, entity.Id); AreEqual(11f, pos.x); break;
                    case 1: AreEqual(13, entity.Id); AreEqual(13f, pos.x); break;
                }
                AreEqual("find-me", entity.GetComponent<IndexedName>().name);
            });
            AreEqual(2, count);
        } { 
            int count = 0;
            query2.ForEachEntity((ref Position pos, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(12, entity.Id); AreEqual(12f, pos.x); break;
                    case 1: AreEqual(13, entity.Id); AreEqual(13f, pos.x); break;
                }
                AreEqual(123, entity.GetComponent<IndexedInt>().value);
            });
            AreEqual(2, count);
        } { 
            var count = 0;
            query3.ForEachEntity((ref Position pos, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(11, entity.Id); AreEqual(11f, pos.x); break;
                    case 1: AreEqual(13, entity.Id); AreEqual(13f, pos.x); break;
                    case 2: AreEqual(12, entity.Id); AreEqual(12f, pos.x); break;
                }
            });
            AreEqual(3, count);
        } {
            var count = 0;
            query4.ForEachEntity((ref Position pos, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, Entity entity) => {
                count++;
                AreEqual(13,        entity.Id);     AreEqual(13f, pos.x);
                AreEqual(cx.target, entity.GetComponent<AttackComponent>().target);
            });
            AreEqual(1, count);
        } {
            var count = 0;
            query5.ForEachEntity((ref Position pos, ref Rotation _, ref MyComponent1 _, ref MyComponent2 _, ref MyComponent3 _, ref MyComponent4 _, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(12, entity.Id); AreEqual(12f, pos.x); break;
                    case 1: AreEqual(13, entity.Id); AreEqual(13f, pos.x); break;
                    case 2: AreEqual(14, entity.Id); AreEqual(14f, pos.x); break;
                }
            });
            AreEqual(3, count);
        } {
            var count = 0;
            foreach (var (positions, _, _, _, _, _, entities) in query5.Chunks) {
                AreEqual(1, positions.Length);
                AreEqual(1, entities.Length);
                var pos = positions[0]; // positions.start can be > 0
                var id  = entities [0]; // entities.start  can be > 0
                switch (count++) {
                    case 0: AreEqual(12, id); AreEqual(12f, pos.x); break;
                    case 1: AreEqual(13, id); AreEqual(13f, pos.x); break;
                    case 2: AreEqual(14, id); AreEqual(14f, pos.x); break;
                }
            }
            AreEqual(3, count);
        }
    }
}

}
