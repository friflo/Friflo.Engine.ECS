using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;



partial class TestClass
{
    // [Query]
    public static void MovePosition(ref Position position) {
        position.x += 1;
    }
    
    private static readonly int MovePositionKey = EntityStore.UserDataNewKey();
    
    public static void MovePositionQuery(EntityStore store)
    {
         var query = (ArchetypeQuery<Position>)store.UserDataGet(MovePositionKey);
         if (query == null) {
             query = store.Query<Position>();
             store.UserDataSet(MovePositionKey, query);
         }
         foreach (var (components, entities) in query.Chunks)
         {
             var componentsSpan = components.Span;
             for (int n = 0; n < entities.Length; n++) {
                 MovePosition(ref componentsSpan[n]);
             }
         }
    }
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
        
        TestClass.MovePositionQuery(store);
        AreEqual(2.0f, entity.GetComponent<Position>().x);
    }
}

}

