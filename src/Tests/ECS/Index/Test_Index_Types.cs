using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {

public static class Test_Index_Types
{
    [Test]
    public static void Test_Index_Types_Guid()
    {
        var guid1 = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var guid2 = Guid.Parse("10000000-0000-0000-0000-000000000000");
        
        var store  = new EntityStore();
        var values = store.GetAllIndexedComponentValues<GuidComponent, Guid>();
        
        var query0 = store.Query().HasValue    <GuidComponent, Guid>(guid1);
        var query1 = store.Query().ValueInRange<GuidComponent, Guid>(guid1, guid1);
       
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddComponent(new GuidComponent { guid = guid1 });
        AreEqual(1, query0.Count);
        AreEqual(1, query1.Count);
        AreEqual(1, values.Count);

        entity2.AddComponent(new GuidComponent { guid = guid2 });
        AreEqual(1, query0.Count);
        AreEqual(2, values.Count);
    }
    
    [Test]
    public static void Test_Index_Types_DateTime()
    {
        var date1 = new DateTime(2024, 6, 25);
        var date2 = new DateTime(2024, 6, 26);
        var store = new EntityStore();
        var values = store.GetAllIndexedComponentValues<DateTimeComponent, DateTime>();
        
        var query1 = store.Query().HasValue    <DateTimeComponent, DateTime>(date1);
        var query2 = store.Query().ValueInRange<DateTimeComponent, DateTime>(date1, date1);

        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        
        entity1.AddComponent(new DateTimeComponent { dateTime = date1 });
        AreEqual(1, query1.Count);
        AreEqual(1, query2.Count);
        AreEqual(1, values.Count);

        entity2.AddComponent(new DateTimeComponent { dateTime = date2 });
        AreEqual(1, query1.Count);
        AreEqual(1, query2.Count);
        AreEqual(2, values.Count);
    }
    
    [Test]
    public static void Test_Index_Types_enum()
    {
        var store = new EntityStore();
        var values = store.GetAllIndexedComponentValues<EnumComponent, MyEnum>();

        var query1 = store.Query().HasValue    <EnumComponent, MyEnum>(MyEnum.E1);
        
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        entity1.AddComponent(new EnumComponent { value = MyEnum.E0 });
        AreEqual(0, query1.Count);
        AreEqual(1, values.Count);

        entity2.AddComponent(new EnumComponent { value = MyEnum.E1 });
        AreEqual(1, query1.Count);
        AreEqual(2, values.Count);
        
        entity3.AddComponent(new EnumComponent { value = MyEnum.E2 });
        AreEqual(1, query1.Count);
        AreEqual(3, values.Count);
    }
    
   
    [Test]
    public static void Test_Index_Types_enum_comparable()
    {
        var store = new EntityStore();
        var values = store.GetAllIndexedComponentValues<ComparableEnumComponent, ComparableEnum>();

        var query1 = store.Query().HasValue    <ComparableEnumComponent, ComparableEnum>(MyEnum.E1);
        var query2 = store.Query().ValueInRange<ComparableEnumComponent, ComparableEnum>(MyEnum.E1, MyEnum.E2);
        
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        entity1.AddComponent(new ComparableEnumComponent { value = MyEnum.E0 });
        AreEqual(0, query1.Count);
        AreEqual(0, query2.Count);
        AreEqual(1, values.Count);

        entity2.AddComponent(new ComparableEnumComponent { value = MyEnum.E1 });
        AreEqual(1, query1.Count);
        AreEqual(1, query2.Count);
        AreEqual(2, values.Count);
        
        entity3.AddComponent(new ComparableEnumComponent { value = MyEnum.E2 });
        AreEqual(1, query1.Count);
        AreEqual(2, query2.Count);
        AreEqual(3, values.Count);
    }
}

}
