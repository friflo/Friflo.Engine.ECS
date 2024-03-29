using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {


public static class Test_Entity_generic
{
    [Test]
    public static void Test_Entity_Events_OnTagsChanged()
    {
        var store   = new EntityStore();
        
        int tagsCount = 0;
        store.OnTagsChanged += changed => {
            var str = changed.ToString();
            switch (tagsCount++)
            {
                case 0: AreEqual("entity: 1 - event > Add Tags: [#TestTag]", str); break;
            }
        };
        int componentAddedCount = 0;
        store.OnComponentAdded += changed => {
            var str = changed.ToString();
            switch (componentAddedCount++)
            {
                case 0: AreEqual("entity: 1 - event > Add Component: [Position]",       str); break;
                case 1: AreEqual("entity: 1 - event > Add Component: [Scale3]",         str); break;
                case 2: AreEqual("entity: 1 - event > Update Component: [Position]",    str); break;
                case 3: AreEqual("entity: 1 - event > Update Component: [Scale3]",      str); break;
            }
        };
        var entity  = store.CreateEntity();
        var tags    = Tags.Get<TestTag>();
        
        entity.Add(new Position(1,1,1), new Scale3(1,1,1), tags);
        entity.Add(new Position(2,2,2), new Scale3(2,2,2), tags);
        
        AreEqual(1, tagsCount);
        AreEqual(4, componentAddedCount);
    }
}

}