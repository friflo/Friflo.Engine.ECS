using Friflo.Engine.ECS;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index.Query {


public static partial class Test_Index_Query
{

    private static void QueryArg0 (IndexContext cx)
    {
        var store = cx.store;
        var query1  = store.Query().                  HasValue<IndexedName,   string>("find-me");
        var query2  = store.Query().                  HasValue<IndexedInt,    int>   (123);
        var query3  = store.Query().                  HasValue<IndexedName,   string>("find-me").
                                                      HasValue<IndexedInt,    int>   (123);
        var query4  = store.Query().                  HasValue<AttackComponent, Entity>(cx.target);
        var query5  = store.Query().                  ValueInRange<IndexedInt, int>(100, 1000);
        cx.query1 = query1;
        cx.query2 = query2;
        cx.query3 = query3;
        cx.query4 = query4;
        cx.query5 = query5;
    }

}

}
