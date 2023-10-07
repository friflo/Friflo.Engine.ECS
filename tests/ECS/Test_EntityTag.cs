using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

internal struct TestTag : IEntityTag { }

public static class Test_EntityTag
{
    [Test]
    public static void Test_list_EntityTags() {
        var schema      = EntityStore.GetComponentSchema();
        AreEqual(2,     schema.Tags.Length);
        
        var tags = schema.Tags;
        IsNull(tags[0]);
        for (int n = 1; n < tags.Length; n++) {
            var type = tags[n];
            AreEqual(n, type.index);
            AreEqual(ComponentKind.Tag, type.kind);
            IsNull(type.componentKey);
            // var typeHandle = type.type.TypeHandle.Value.ToInt64();
            // AreEqual(typeHandle, type.structHash); todo
        }
        var testTagType = schema.EntityTagByType[typeof(TestTag)];
        AreEqual(typeof(TestTag), testTagType.type);
        AreEqual("entity tag: #TestTag", testTagType.ToString());
    }
}
