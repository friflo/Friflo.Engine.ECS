// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

internal enum FieldDataKind
{
    Component   = 0,
    Member      = 1
}

internal readonly struct FieldData
{
    private  readonly   FieldDataKind   kind;
    internal readonly   object          instance;
    internal readonly   Var.Member      member;
    
    internal FieldData(object component) {
        kind        = FieldDataKind.Component;
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


internal readonly struct ComponentField
{
#region internal fields
    private  readonly   string      path;
    internal readonly   string      name;
    internal readonly   Control     control;
    private  readonly   Type        type;
    private  readonly   int         index;
    private  readonly   Var.Member  member;

    public   override   string  ToString() => path;

    #endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    private ComponentField(string parent, string name, Type type, int index, Control control, Var.Member member) {
        this.name       = name;
        this.control    = control;
        path            = parent == null ? name : $"{parent}.{name}"; 
        this.member     = member;
        this.type       = type;
        this.index      = index;
    }
    
    internal static bool AddComponentFields(
        List<ComponentField>    componentFields,
        Type                    type,
        string                  parent,
        string                  fieldName,
        Var.Member              member)
    {
        if (type == typeof(Position)) {
            fieldName     ??= nameof(Position.value);
            componentFields.Add(new ComponentField(parent, fieldName,   typeof(Position), 0, new Vector3Field(), member));
            return true;
        }
        if (type == typeof(Transform)) {
            componentFields.Add(new ComponentField(parent, "position",   typeof(Transform), 0, new Vector3Field(), member));
            componentFields.Add(new ComponentField(parent, "rotation",   typeof(Transform), 1, new Vector3Field(), member));
            return true;
        }
        if (type == typeof(EntityName)) {
            var control     = new StringField();
            fieldName     ??= nameof(EntityName.value);
            componentFields.Add(new ComponentField(parent, fieldName,   typeof(EntityName), 0, control, member));
            return true;
        }
        return false;
    }
        
    internal static void AddScriptFields(List<ComponentField> componentFields, Type type)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var fields      = classMapper.PropFields.fields;
        for (int n = 0; n < fields.Length; n++)
        {
            var propField   = fields[n];
            var fieldType   = propField.fieldType.type;
            var member      = classMapper.GetMember(propField.name);
            if (AddComponentFields(componentFields, fieldType, null, propField.name, member)) {
                continue;
            }
            var control     = CreateField(fieldType);
            componentFields.Add(new ComponentField(null, propField.name, fieldType, 0, control, member));
        }
    }
    
    private static Control CreateField (Type fieldType)
    {
        if (fieldType == typeof(string)) {
            return new StringField();
        }
        if (fieldType == typeof(int)) {
            return new StringField();
        } else {
            return new StringField();
        }
    }
    
    internal static void SetComponentFields(ComponentField[] componentFields, object instance)
    {
        var data = new FieldData(instance);
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
            var position    = (Position)data.GetData();
            control.X = position.x;
            control.Y = position.y;
            control.Z = position.z;
            return;
        }
        if (type == typeof(Transform)) {
            var control     = (Vector3Field)field.control; 
            var transform   = (Transform)data.GetData();
            if (field.index == 0) {
                control.X = transform.m11;
                control.Y = transform.m12;
                control.Z = transform.m13;
                return;
            }
            control.X = transform.m21;
            control.Y = transform.m22;
            control.Z = transform.m23;
            return;
        }
        if (type == typeof(EntityName)) {
            var control     = (StringField)field.control; 
            var entityName  = (EntityName)data.GetData();
            control.Value   = entityName.value;
            return;
        }
        if (type == typeof(string)) {
            var control     = (StringField)field.control; 
            control.Value   = data.member.GetVar(data.instance).String;
            return;
        }
        if (type == typeof(int)) {
            var control     = (StringField)field.control; 
            control.Value   = data.member.GetVar(data.instance).Int32.ToString();
            return;
        }
        throw new InvalidOperationException($"missing field assignment. field: {field.name}");
    }
}
