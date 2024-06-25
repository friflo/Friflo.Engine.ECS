using Friflo.Engine.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {


internal static class Test_Index_Args
{
    internal static void QueryArg2 (IndexContext cx)
    {
        var store = cx.store;
        var query1  = store.Query<Position,    IndexedName>().  HasValue<IndexedName,   string>("find-me");
        var query2  = store.Query<Position,    IndexedInt>().   HasValue<IndexedInt,    int>   (123);
        var query3  = store.Query<IndexedName, IndexedInt>().   HasValue<IndexedName,   string>("find-me").
                                                                HasValue<IndexedInt,    int>   (123);
        var query4  = store.Query().                            HasValue<IndexedName,   string>("find-me").
                                                                HasValue<IndexedInt,    int>   (123);
        var query5  = store.Query<Position, AttackComponent>(). HasValue<AttackComponent, Entity>(cx.target);
        cx.query1 = query1;
        cx.query2 = query2;
        cx.query3 = query3;
        cx.query4 = query4;
        {
            int count = 0;
            query1.ForEachEntity((ref Position _, ref IndexedName indexedName, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(11, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                }
                AreEqual("find-me", indexedName.name);
            });
            AreEqual(2, count);
        } { 
            int count = 0;
            query2.ForEachEntity((ref Position _, ref IndexedInt indexedInt, Entity entity) => {
                switch (count++) {
                    case 0: AreEqual(12, entity.Id); break;
                    case 1: AreEqual(13, entity.Id); break;
                }
                AreEqual(123, indexedInt.value);
            });
            AreEqual(2, count);
        } { 
            var count = 0;
            query3.ForEachEntity((ref IndexedName _, ref IndexedInt _, Entity entity) => {
                AreEqual(13, entity.Id);
                count++;
            });
            AreEqual(1, count);
        } {
            var count = 0;
            query5.ForEachEntity((ref Position _, ref AttackComponent attack, Entity entity) => {
                count++;
                AreEqual(13,        entity.Id);
                AreEqual(cx.target, attack.target);
            });
            AreEqual(1, count);
        }
    }
}

}
