using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

public static class Test_SchemaType
{
    /// <summary>
    /// Ensure initialization of <see cref="ScriptType{T}.Index"/>.
    /// Especially if <see cref="Tags.Get{T}"/> is the first call in an application.  
    /// </summary>
    [Test]
    public static void Test_SchemaType_Script_Index()
    {
        var scriptIndex = ScriptType<TestScript1>.Index;
        var schema      = EntityStore.GetEntitySchema();
        var scriptType   = schema.scripts[scriptIndex];
        
        AreEqual("TestScript1",         scriptType.Name);
        AreEqual(scriptIndex,           scriptType.ScriptIndex);
        AreEqual(typeof(TestScript1),   scriptType.Type);
    }

    /// <summary>
    /// Ensure initialization of <see cref="TagType{T}.TagIndex"/>.
    /// </summary>
    [Test]
    public static void Test_SchemaType_Tag_Index()
    {
        var tagIndex    = TagType<TestTag>.TagIndex;
        var schema      = EntityStore.GetEntitySchema();
        var tagType     = schema.tags[tagIndex];
        
        AreEqual("TestTag",         tagType.Name);
        AreEqual(tagIndex,          tagType.TagIndex);
        AreEqual(typeof(TestTag),   tagType.Type);
    }
}

}
