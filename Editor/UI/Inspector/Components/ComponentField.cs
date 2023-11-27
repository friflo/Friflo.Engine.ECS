// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Editor.UI.Inspector.FieldDataKind;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable RedundantJumpStatement
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

public interface IFieldControl
{
    ComponentField  ComponentField { get; init; }  
} 

public class ComponentField
{
#region internal fields
    internal readonly   string      name;
    internal            Control     control;
    private             FieldData   data;
    private  readonly   Type        type;
    private  readonly   int         index;
    private  readonly   Var.Member  member;

    public   override   string      ToString() => name;

    #endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    private ComponentField(string name, Type type, int index, Var.Member member) {
        this.name       = name;
        this.member     = member;
        this.type       = type;
        this.index      = index;
    }
    
#region create component fields
    internal static bool AddComponentFields(
        List<ComponentField>    fields,
        Type                    type,
        string                  fieldName,
        Var.Member              member)
    {
        if (type == typeof(Position)) {
            fieldName     ??= nameof(Position.value);
            var field       = new ComponentField(fieldName,    typeof(Position),   0, member);
            field.control   = new Vector3Field { ComponentField = field };
            fields.Add(field);
            return true;
        }
        if (type == typeof(Transform)) {
            var field0      = new ComponentField("position",   typeof(Transform),  0, member);
            var field1      = new ComponentField("rotation",   typeof(Transform),  1, member);
            field0.control  = new Vector3Field { ComponentField = field0 };
            field1.control  = new Vector3Field { ComponentField = field1 };
            fields.Add(field0);
            fields.Add(field1);
            return true;
        }
        if (type == typeof(EntityName)) {
            fieldName     ??= nameof(EntityName.value);
            var field       = new ComponentField(fieldName,    typeof(EntityName), 0, member);
            field.control   = new StringField { ComponentField = field };
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
            if (AddComponentFields(fields, fieldType, propField.name, member)) {
                continue;
            }
            var field       = new ComponentField(propField.name, fieldType, 0, member);
            field.control   = CreateField(fieldType, field); 
            fields.Add(field);
        }
    }
    
    private static Control CreateField (Type fieldType, ComponentField field)
    {
        if (fieldType == typeof(string)) {
            return new StringField { ComponentField = field };
        }
        if (fieldType == typeof(int)) {
            return new StringField { ComponentField = field };
        } else {
            return new StringField { ComponentField = field };
        }
    }
    #endregion
    
#region read component values
    internal static void SetComponentFields(ComponentField[] componentFields, Entity entity, IComponent component)
    {
        var data = new FieldData(entity, component);
        foreach (var field in componentFields) {
            field.data  = default; // clear to prevent update calls
            SetComponentField(field, data);
            field.data  = data;
        }
    }
    
    internal static void SetScriptFields(ComponentField[] componentFields, Script script)
    {
        foreach (var field in componentFields) {
            field.data  = default; // clear to prevent update calls
            var data    = new FieldData(script, field.member);
            SetComponentField(field, data);
            field.data  = data;
        }
    }
    
    private static void SetComponentField(ComponentField field, FieldData data)
    {
        var type    = field.type;
        if (type == typeof(Position)) {
            var control     = (Vector3Field)field.control; 
            var position    = (Position)data.GetData();
            control.X       = position.x;
            control.Y       = position.y;
            control.Z       = position.z;
            return;
        }
        if (type == typeof(Transform)) {
            var control     = (Vector3Field)field.control;
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
    #endregion
    
    // ------------------------------ change component / script field ------------------------------ 
#region set vector
    private static readonly bool LogChanges = false;

    internal void SetVector(in Vector3 vector)
    {
        if (LogChanges && data.kind != None)  Console.WriteLine($"--- set vector: {vector}");
        
        switch (data.kind) {
            case None:
                return;
            case Component:
                SetComponentVector(data.entity, vector);
                return;
            case Member:
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
    internal void SetString(string value)
    {
        if (LogChanges && data.kind != None)  Console.WriteLine($"--- set string: {value}");
        
        switch (data.kind) {
            case None:
                return;
            case Component:
                SetComponentString(data.entity, value);
                return;
            case Member:
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
