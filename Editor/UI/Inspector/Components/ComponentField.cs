// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable RedundantJumpStatement
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

internal enum FieldDataKind
{
    None        = 0,
    Component   = 1,
    Member      = 2
}

internal readonly struct FieldData
{
    internal readonly   FieldDataKind   kind;
    internal readonly   Entity          entity;
    internal readonly   object          instance;
    internal readonly   Var.Member      member;
    
    internal FieldData(Entity entity, object component) {
        kind        = FieldDataKind.Component;
        this.entity = entity; 
        instance    = component;
    }
    
    internal FieldData(Script instance, Var.Member member) {
        kind            = FieldDataKind.Member;
        this.instance   = instance;
        this.member     = member;
    }
    
    internal object GetData() {
        if (kind == FieldDataKind.Component) {
            return instance;
        }
        return member.GetVar(instance).Object;
    }
}


internal class ComponentField
{
#region internal fields
    private  readonly   string      path;
    internal readonly   string      name;
    internal            Control     control;
    private  readonly   Type        type;
    private  readonly   int         index;
    private  readonly   Var.Member  member;

    public   override   string  ToString() => path;

    #endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    private ComponentField(string parent, string name, Type type, int index, Var.Member member) {
        this.name       = name;
        path            = parent == null ? name : $"{parent}.{name}"; 
        this.member     = member;
        this.type       = type;
        this.index      = index;
    }
    
    internal static bool AddComponentFields(
        List<ComponentField>    fields,
        Type                    type,
        string                  parent,
        string                  fieldName,
        Var.Member              member)
    {
        if (type == typeof(Position)) {
            fieldName     ??= nameof(Position.value);
            var field       = new ComponentField(parent, fieldName,    typeof(Position),   0, member);
            field.control   = new Vector3Field(field);
            fields.Add(field);
            return true;
        }
        if (type == typeof(Transform)) {
            var field0      = new ComponentField(parent, "position",   typeof(Transform),  0, member);
            var field1      = new ComponentField(parent, "rotation",   typeof(Transform),  1, member);
            field0.control  = new Vector3Field(field0);
            field1.control  = new Vector3Field(field1);
            fields.Add(field0);
            fields.Add(field1);
            return true;
        }
        if (type == typeof(EntityName)) {
            fieldName     ??= nameof(EntityName.value);
            var field       = new ComponentField(parent, fieldName,    typeof(EntityName), 0, member);
            field.control   = new StringField(field);
            fields.Add(field);
            return true;
        }
        return false;
    }
        
    internal static void AddScriptFields(List<ComponentField> fields, Type type)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var propFields  = classMapper.PropFields.fields;
        for (int n = 0; n < propFields.Length; n++)
        {
            var propField   = propFields[n];
            var fieldType   = propField.fieldType.type;
            var member      = classMapper.GetMember(propField.name);
            if (AddComponentFields(fields, fieldType, null, propField.name, member)) {
                continue;
            }
            var field       = new ComponentField(null, propField.name, fieldType, 0, member);
            field.control   = CreateField(fieldType, field); 
            fields.Add(field);
        }
    }
    
    private static Control CreateField (Type fieldType, ComponentField field)
    {
        if (fieldType == typeof(string)) {
            return new StringField(field);
        }
        if (fieldType == typeof(int)) {
            return new StringField(field);
        } else {
            return new StringField(field);
        }
    }
    
    internal static void SetComponentFields(ComponentField[] componentFields, Entity entity, object component)
    {
        var data = new FieldData(entity, component);
        foreach (var field in componentFields) {
            SetComponentField(field, data);
        }
    }
    
    internal static void SetScriptFields(ComponentField[] componentFields, Script script)
    {
        foreach (var field in componentFields) {
            var data = new FieldData(script, field.member);
            SetComponentField(field, data);
        }
    }
    
    private static void SetComponentField(ComponentField field, FieldData data)
    {
        var type = field.type;
        if (type == typeof(Position)) {
            var control     = (Vector3Field)field.control; 
            control.data    = default; // clear to prevent update calls
            var position    = (Position)data.GetData();
            control.X       = position.x;
            control.Y       = position.y;
            control.Z       = position.z;
            control.data    = data;
            return;
        }
        if (type == typeof(Transform)) {
            var control     = (Vector3Field)field.control;
            control.data    = default; // clear to prevent update calls
            var transform   = (Transform)data.GetData();
            if (field.index == 0) {
                control.X = transform.m11;
                control.Y = transform.m12;
                control.Z = transform.m13;
            } else {
                control.X = transform.m21;
                control.Y = transform.m22;
                control.Z = transform.m23;
            }
            control.data    = data;
            return;
        }
        if (type == typeof(EntityName)) {
            var control     = (StringField)field.control;
            var entityName  = (EntityName)data.GetData();
            control.data    = default; // clear to prevent update calls
            control.Value   = entityName.value;
            control.data    = data;
            return;
        }
        if (type == typeof(string)) {
            var control     = (StringField)field.control;
            control.data    = default; // clear to prevent update calls
            control.Value   = data.member.GetVar(data.instance).String;
            control.data    = data;
            return;
        }
        if (type == typeof(int)) {
            var control     = (StringField)field.control;
            control.data    = default; // clear to prevent update calls
            control.Value   = data.member.GetVar(data.instance).Int32.ToString();
            control.data    = data;
            return;
        }
        throw new InvalidOperationException($"missing field assignment. field: {field.name}");
    }
    
    // ------------------------------ change component / script field ------------------------------ 
#region set vector
    internal void SetVector(in FieldData data, in Vector3 vector)
    {
        switch (data.kind) {
            case FieldDataKind.None:
                return;
            case FieldDataKind.Component:
                SetComponentVector(data.entity, vector);
                return;
            case FieldDataKind.Member:
                SetScriptVector(data.instance, vector);
                return;
        }
    }
    
    private void SetComponentVector(Entity entity, in Vector3 vector)
    {
        if (type == typeof(Transform))
        {
            ref var transform = ref entity.GetComponent<Transform>();
            switch (index) {
                case 0:
                    transform.m11 = vector.X;
                    transform.m12 = vector.Y;
                    transform.m13 = vector.Z;
                    return;
                case 1:
                    transform.m21 = vector.X;
                    transform.m22 = vector.Y;
                    transform.m23 = vector.Z;
                    return;
            }
            return;
        }
        if (type == typeof(Position))
        {
            ref var position = ref entity.GetComponent<Position>();
            position.value = vector;
            return;
        }
    }
    
    private void SetScriptVector(object script, in Vector3 vector)
    {
        if (type == typeof(Transform))
        {
            member.SetVar(script, new Var(vector));
            return;
        }
        if (type == typeof(Position))
        {
            var var = new Var(new Position { value = vector});
            member.SetVar(script, var);
            return;
        }
    }
    #endregion
    
#region set string
    internal void SetString(in FieldData data, string value) {
        switch (data.kind) {
            case FieldDataKind.None:
                return;
            case FieldDataKind.Component:
                SetComponentString(data.entity, value);
                return;
            case FieldDataKind.Member:
                SetScriptString(data.instance, value);
                return;
        }
    }
    
    private void SetComponentString(Entity entity, string value)
    {
        if (type == typeof(EntityName)) {
            entity.AddComponent(new EntityName(value));
            return;
        }
    }
    
    private void SetScriptString(object script, string value)
    {
        if (type == typeof(EntityName)) {
             member.SetVar(script, new Var(value));
            return;
        }
        if (type == typeof(string)) {
            member.SetVar(script, new Var(value));
            return;
        }
        if (type == typeof(int)) {
            var i32     = int.Parse(value);
            member.SetVar(script, new Var(i32));
            return;
        }
    }
    #endregion
}
