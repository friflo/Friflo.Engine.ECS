using System;
using System.Reflection;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable EqualExpressionComparison
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {

public static class Test_MemberPath
{
    [Test]
    public static void Test_MemberPath_GetEntityComponentSelf()
    {
        var store  = new EntityStore();        
        var entity = store.CreateEntity(new EntityName("self-1"));
        var selfInfo = MemberPath.Get(typeof(EntityName), "");
        
        AreEqual("",                            selfInfo.path);
        AreEqual("EntityName",                  selfInfo.name);
        AreEqual(typeof(EntityName),            selfInfo.memberType);
        AreEqual(typeof(EntityName),            selfInfo.declaringType);
        AreEqual(typeof(EntityName),            selfInfo.componentType.Type);
        AreEqual("get: (EntityName => this)",   selfInfo.getter.Method.Name);
        AreEqual("set: (EntityName => this)",   selfInfo.setter.Method.Name);
        IsNull  (                               selfInfo.memberInfo);
        AreEqual("EntityName",                  selfInfo.ToString());
        IsTrue(EntityUtils.GetEntityComponentMember<EntityName>(entity, selfInfo, out var self, out _));
        AreEqual("self-1",                      self.value);
        OnMemberChanged<EntityName> handler = (ref EntityName value, Entity entity1, string path, in EntityName old) => {
            AreEqual(1,     entity1.Id);
            AreEqual("",    path);
            AreEqual("self-2", value.value);
            AreEqual("self-1", old.value);
        };
        IsTrue(EntityUtils.SetEntityComponentMember(entity, selfInfo, new EntityName("self-2"), handler, out _));
        AreEqual("self-2",                      entity.GetComponent<EntityName>().value);
    }
    
    [Test]
    public static void Test_MemberPath_EntityComponentField_errors()
    {
        var store       = new EntityStore();        
        var entity      = store.CreateEntity(new MemberExceptionComponent());
        var valuePath   = MemberPath.Get(typeof(MemberExceptionComponent), nameof(MemberExceptionComponent.value));
        
        IsFalse(EntityUtils.GetEntityComponentMember<int>(entity, valuePath, out var value, out var exception));
        AreEqual(0, value);
        IsTrue(typeof(InvalidOperationException) == exception.GetType());
        AreEqual("get", exception.Message);
        
        IsFalse(EntityUtils.SetEntityComponentMember(entity, valuePath, 42, null, out exception));
        IsTrue(typeof(InvalidOperationException) == exception.GetType());
        AreEqual("set", exception.Message);
    }
    
    
    [Test]
    public static void Test_MemberPath_GetEntityComponentField()
    {
        var store  = new EntityStore();        
        var entity = store.CreateEntity(new EntityName("comp-value"));
        var componentType   = typeof(EntityName);
        
        entity.GetComponent<EntityName>().value = "comp-value";
        var nameInfo = MemberPath.Get(componentType, nameof(EntityName.value));
        IsTrue(EntityUtils.GetEntityComponentMember<string>(entity, nameInfo, out var name, out var exception));
        IsNull  (exception);
        AreEqual("comp-value",                      name);
        AreEqual("value",                           nameInfo.path);
        AreEqual("value",                           nameInfo.name);
        AreEqual(typeof(string),                    nameInfo.memberType);
        AreEqual(typeof(EntityName),                nameInfo.declaringType);
        AreEqual(typeof(EntityName),                nameInfo.componentType.Type);
        AreEqual("get: (EntityName => value)",      nameInfo.getter.Method.Name);
        AreEqual("set: (EntityName => value)",      nameInfo.setter.Method.Name);
        NotNull ("value",                           nameInfo.memberInfo.Name);
        AreEqual("EntityName value : String",       nameInfo.ToString());
        
        var nameLengthInfo  = MemberPath.Get(componentType, "value.Length");
        IsTrue(EntityUtils.GetEntityComponentMember<int>(entity, nameLengthInfo, out var length, out  exception));
        IsNull  (exception);
        AreEqual("comp-value".Length,               length);
        AreEqual("value.Length",                    nameLengthInfo.path);
        AreEqual("Length",                          nameLengthInfo.name);
        AreEqual(typeof(int),                       nameLengthInfo.memberType);
        AreEqual(typeof(EntityName),                nameLengthInfo.declaringType);
        AreEqual(typeof(EntityName),                nameLengthInfo.componentType.Type);
        NotNull ("Length",                          nameLengthInfo.memberInfo.Name);
        AreEqual("EntityName value.Length : Int32", nameLengthInfo.ToString());
        
        var start = Mem.GetAllocatedBytes();
        for (int n = 0; n < 10; n++) {
            EntityUtils.GetEntityComponentMember<string>(entity, nameInfo, out _, out _);
        }
        Mem.AssertNoAlloc(start);
        
        IsTrue(EntityUtils.SetEntityComponentMember(entity, nameInfo, "changed", null, out exception));
        IsNull  (exception);
        AreEqual("changed", entity.GetComponent<EntityName>().value);
        
        start = Mem.GetAllocatedBytes();
        for (int n = 0; n < 10; n++) {
            EntityUtils.SetEntityComponentMember(entity, nameInfo, "changed 2", null, out _);
        }
        Mem.AssertNoAlloc(start);
        AreEqual("changed 2", entity.GetComponent<EntityName>().value);
    }
    
    [Test]
    public static void Test_MemberPath_GetEntityComponentField_ref()
    {
        var store  = new EntityStore();        
        var entity = store.CreateEntity(new EntityName("comp-value"));
        var schema          = EntityStore.GetEntitySchema();
        var componentType   = schema.ComponentTypeByType[typeof(EntityName)];
        var mi = (FieldInfo)typeof(EntityName).GetMember("value")[0];
        
        for (int n = 0; n < 10; n++) {
            var component = EntityUtils.GetEntityComponent(entity, componentType);
            mi.GetValue(component);
        }
    }
    
    [Test]
    public static void Test_MemberPath_Get_exceptions()
    {
        var e1 = Throws<InvalidOperationException>(() => {
            MemberPath.Get(typeof(EntityName), "unknown");
        });
        AreEqual("Member 'unknown' not found in Type 'EntityName'", e1!.Message);
        
        var store       = new EntityStore();        
        var entity      = store.CreateEntity(new EntityName("some name"));
        var nameInfo    = MemberPath.Get(typeof(EntityName), nameof(EntityName.value));
        
        var e2 = Throws<InvalidCastException>(() => {
            EntityUtils.GetEntityComponentMember<int>(entity, nameInfo, out _, out _);
        });
        StringAssert.StartsWith("Unable to cast object of type", e2!.Message);

        var e3 = Throws<InvalidCastException>(() => {
            EntityUtils.SetEntityComponentMember(entity, nameInfo, 42, null, out _);
        });
        StringAssert.StartsWith("Unable to cast object of type", e3!.Message);
        
        var e4 = Throws<InvalidOperationException>(() => {
            MemberPath.Get(typeof(EntityName), " name");
        });
        AreEqual("Member ' name' not found in Type 'EntityName'", e4!.Message);
    }
    
    [Test]
    public static void Test_MemberPath_Get_ref_return()
    {
        var tags = MemberPath.Get(typeof(Entity), nameof(Entity.Tags));
        IsNull(tags.getter);
        IsNull(tags.setter);
        AreEqual(typeof(Tags).MakeByRefType(), tags.memberType);
    }
    
#pragma warning disable CS0649   // Field ... is never assigned to, and will always have its default value

    class ClassAccess
    {
        internal            int  field;
        internal readonly   int  fieldReadonly;
        internal            int  property           { get; set; }
        internal            int  propertyReadonly   { get; }
        
        internal            StructAccess structField;
        internal readonly   StructAccess structFieldReadonly;
        internal            StructAccess structProperty         { get; set; }
        internal            StructAccess structPropertyReadonly { get; }
    }
    
    struct StructAccess
    {
        internal            int  field;
    }
    
    [Test]
    public static void Test_MemberPath_readonly()
    {
        {
            var path = MemberPath.Get(typeof(ClassAccess), nameof(ClassAccess.field));
            NotNull(path.getter);
            NotNull(path.setter);
        } {
            var path = MemberPath.Get(typeof(ClassAccess), nameof(ClassAccess.fieldReadonly));
            NotNull(path.getter);
            IsNull (path.setter);
        } {
            var path = MemberPath.Get(typeof(ClassAccess), nameof(ClassAccess.property));
            NotNull(path.getter);
            NotNull(path.setter);
        } {
            var path = MemberPath.Get(typeof(ClassAccess), nameof(ClassAccess.propertyReadonly));
            NotNull(path.getter);
            IsNull (path.setter);
        }
        // --- struct
        {
            var path = MemberPath.Get(typeof(ClassAccess), "structField.field");
            NotNull(path.getter);
            NotNull(path.setter);
        } {
            var path = MemberPath.Get(typeof(ClassAccess), "structFieldReadonly.field");
            NotNull(path.getter);
            IsNull (path.setter);
        } {
            var path = MemberPath.Get(typeof(ClassAccess), "structProperty.field");
            NotNull(path.getter);
            NotNull(path.setter);
        } {
            var path = MemberPath.Get(typeof(ClassAccess), "structPropertyReadonly.field");
            NotNull(path.getter);
            IsNull (path.setter);
        }
    }
}

}
