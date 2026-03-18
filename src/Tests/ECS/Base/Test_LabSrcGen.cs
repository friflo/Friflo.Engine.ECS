using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using Tests.Utils;
using static NUnit.Framework.Assert;



partial class TestClass
{
    [Query]
    [AllComponents<MyComponent1>]
    [AnyComponents<MyComponent2>]
    [WithoutAllComponents<MyComponent3>]
    [WithoutAnyComponents<MyComponent4>]
    [AllTags<TestTag>]
    [AnyTags<TestTag2>]
    [WithoutAllTags<TestTag3>]
    [WithoutAnyTags<TestTag4>]
    public static void MovePosition(ref Position position) {
        position.x += 1;
    }
    
#region will be generated
    private static readonly int MovePositionSlot = EntityStore.UserDataNewSlot();
    
    public static ArchetypeQuery MovePositionQuery(EntityStore store)
    {
         var query = (ArchetypeQuery<Position>)store.UserDataGet(MovePositionSlot);
         if (query == null) {
             query = store.Query<Position>();
             store.UserDataSet(MovePositionSlot, query);
         }
         foreach (var (components, entities) in query.Chunks)
         {
             var componentsSpan = components.Span;
             for (int n = 0; n < entities.Length; n++) {
                 MovePosition(ref componentsSpan[n]);
             }
         }
         return query;
    }
    #endregion
}


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {

public static class Test_SrcGen
{
    [Test]
    public static void Test_SrcGen_Call() {
        var store = new EntityStore();
        var entity = store.CreateEntity(new Position());
        
        TestClass.MovePositionQuery(store);
        AreEqual(1.0f, entity.GetComponent<Position>().x);
        
        var start = Mem.GetAllocatedBytes();
        TestClass.MovePositionQuery(store);
        Mem.AssertNoAlloc(start);
        AreEqual(2.0f, entity.GetComponent<Position>().x);
    }
}

}

