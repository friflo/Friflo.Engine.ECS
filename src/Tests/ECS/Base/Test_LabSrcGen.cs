using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;



partial class TestClass
{
    public void MyMethod() {}
   
    // [Query]
    public static void MovePosition(ref Position position) {
        position.y += 1;
    }
    
    private static readonly int queryKey = 1; // EntityStore.CreateQueryKey();
    
    public static void MovePositionQuery(EntityStore store)
    {
         var query = (ArchetypeQuery<Position>)null; // store.GetCachedQuery(queryKey);
         if (query == null) {
             query = store.Query<Position>();
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
        TestClass.MovePositionQuery(store);
    }
}

}

