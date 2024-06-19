using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base {

public static class Test_Components
{
    [Test]
    public static void Test_Components_Equality()
    {
        IsTrue  (new Position()         == default);
        IsFalse (new Position()         != default);
        IsTrue  (new Position().Equals(default));
        AreEqual("1, 2, 3", new Position(1, 2, 3).ToString());
        
        IsTrue  (new Rotation()         == default);
        IsFalse (new Rotation()         != default);
        IsTrue  (new Rotation().Equals(default));
        AreEqual("1, 2, 3, 4", new Rotation(1, 2, 3, 4).ToString());
        
        IsTrue  (new Scale3()           == default);
        IsFalse (new Scale3()           != default);
        IsTrue  (new Scale3().Equals(default));
        AreEqual("1, 2, 3", new Scale3(1, 2, 3).ToString());
    }
    
    [Test]
    public static void Test_Generic_Component()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        entity.AddComponent(new GenericComponent<int> { value = 37 });
        AreEqual(37, entity.GetComponent<GenericComponent<int>>().value);
        
        entity.AddComponent(new GenericComponent3<int,int,int> { value1 = 1, value2 = 2, value3 = 3});
        var expect = new GenericComponent3<int,int,int> { value1 = 1, value2 = 2, value3 = 3 };
        AreEqual(expect , entity.GetComponent<GenericComponent3<int,int,int>>());
        
        entity.AddTag<GenericTag<int>>();
        IsTrue(entity.Tags.Has<GenericTag<int>>());
        
        entity.AddTag<GenericTag2<int,bool>>();
        IsTrue(entity.Tags.Has<GenericTag2<int,bool>>());
        
        entity.AddTag<GenericTag3<int,int,int>>();
        IsTrue(entity.Tags.Has<GenericTag3<int,int,int>>());
    }
    
    [Test]
    public static void Test_Generic_Component_exceptions()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        var e = Throws<TypeInitializationException>(() => {
            entity.AddComponent(new GenericComponent<long>());
        });
        var inner = e!.InnerException as InvalidOperationException;
        AreEqual("Missing attribute [GenericInstanceType(\"<key>\", typeof(Int64))] for generic IComponent type: Tests.ECS.GenericComponent`1[System.Int64]", inner!.Message);

        e = Throws<TypeInitializationException>(() => {
            entity.AddTag<GenericTag2<int, string>>();
        });
        inner = e!.InnerException as InvalidOperationException;
        AreEqual("Missing attribute [GenericInstanceType(\"<key>\", typeof(Int32), typeof(String))] for generic ITag type: Tests.ECS.GenericTag2`2[System.Int32,System.String]", inner!.Message);
    }
}

}