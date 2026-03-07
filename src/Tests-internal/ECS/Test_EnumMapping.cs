using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

    
enum TagMappingCSharp11
{
    Undefined = 0,
    // generic attributes requires C# 11 or higher
    [MapTag<TestTag>]   TestTag  = 1,
    [MapTag<TestTag2>]  TestTag2 = 2,
}

enum ComponentMappingCSharp11
{
    Undefined = 0,
    // generic attributes requires C# 11 or higher
    [MapComponent<MyComponent1>]    MyComponent1 = 1,
    [MapComponent<MyComponent2>]    MyComponent2 = 2,
}

public static class Test_EnumMapping
{

    [Test]
    public static void Test_EnumMapping_Tags()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.AddTags(Tags.Get<TestTag, TestTag2, TestTag3>());
        
        bool foundUndefined = false;
        bool foundTestTag   = false;
        bool foundTestTag2  = false;

        foreach (var tag in entity.Tags)
        {
            var tagId = tag.AsEnum<TagMappingCSharp11>();
            switch (tagId) {
                case TagMappingCSharp11.TestTag:
                    foundTestTag = true;
                    break;
                case TagMappingCSharp11.TestTag2:
                    foundTestTag2 = true;
                    break;
                default:
                    foundUndefined = true;
                    break;
            }
        }
        IsTrue(foundUndefined);
        IsTrue(foundTestTag);
        IsTrue(foundTestTag2);
    }
	
    [Test]
    public static void Test_EnumMapping_Components()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        entity.Add(new MyComponent1(), new MyComponent2(), new MyComponent3());
        
        bool foundUndefined     = false;
        bool foundMyComponent1  = false;
        bool foundMyComponent2  = false;

        foreach (var component in entity.Components)
        {
            var tagId = component.Type.AsEnum<ComponentMappingCSharp11>();
            switch (tagId) {
                case ComponentMappingCSharp11.MyComponent1:
                    foundMyComponent1 = true;
                    break;
                case ComponentMappingCSharp11.MyComponent2:
                    foundMyComponent2 = true;
                    break;
                default:
                    foundUndefined = true;
                    break;
            }
        }
        IsTrue(foundUndefined);
        IsTrue(foundMyComponent1);
        IsTrue(foundMyComponent2);
    }
}

}
