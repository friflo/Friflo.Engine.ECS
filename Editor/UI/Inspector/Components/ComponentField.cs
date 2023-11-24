// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

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
    private readonly    FieldDataKind   kind;
    private readonly    object          instance;
    private readonly    Var.Member      member;
    
    internal FieldData(object component) {
        kind        = FieldDataKind.Component;
        instance    = component;
    }
    
    internal FieldData(object instance, Var.Member member) {
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
    internal readonly   string  name;
    internal readonly   Control control;
#endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    private ComponentField(string name, Control control) {
        this.name       = name;
        this.control    = control;
    }
    
    internal static bool AddComponentFields(
        List<ComponentField>    componentFields,
        Type                    type,
        FieldData               data,
        string                  fieldName)
    {
        if (type == typeof(Position)) {
            var position    = (Position)data.GetData();
            fieldName     ??= "Value";
            componentFields.Add(new ComponentField(fieldName,   new Vector3Field { vector = position.value }));
            return true;
        }
        if (type == typeof(Transform)) {
            var t           = (Transform)data.GetData();
            var position    = new Vector3(t.m11, t.m12, t.m13);
            var rotation    = new Vector3(t.m21, t.m22, t.m23);
            componentFields.Add(new ComponentField("Position",  new Vector3Field { vector = position }));
            componentFields.Add(new ComponentField("Rotation",  new Vector3Field { vector = rotation }));
            return true;
        }
        if (type == typeof(EntityName)) {
            var name        = (EntityName)data.GetData();
            var control     = new StringField { Value = name.value };
            fieldName     ??= "Value";
            componentFields.Add(new ComponentField(fieldName, control));
            return true;
        }
        return false;
    }
        
    internal static void AddFields(List<ComponentField> componentFields, Type type, object instance)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var fields      = classMapper.PropFields.fields;
        for (int n = 0; n < fields.Length; n++)
        {
            var propField   = fields[n];
            var fieldType   = propField.fieldType.type;
            var member      = classMapper.GetMember(propField.name);
            var data        = new FieldData(instance, member);
            if (AddComponentFields(componentFields, fieldType, data, propField.name)) {
                continue;
            }
            var value       = member.GetVar(instance);
            var control     = CreateField(fieldType, value);
            componentFields.Add(new ComponentField(propField.name, control));
        }
    }
    
    private static Control CreateField (Type fieldType, Var var)
    {
        if (fieldType == typeof(string)) {
            var value = var.String;
            return new StringField { Value = value };
        }
        if (fieldType == typeof(int)) {
            var value = var.Int32; 
            return new StringField { Value = value.ToString() };
        } else {
            var value = var.AsString();
            return new StringField { Value = value };
        }
    }
}
