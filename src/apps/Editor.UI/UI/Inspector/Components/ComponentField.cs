// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Avalonia.Controls;
using Friflo.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Editor.UI.Inspector.FieldDataKind;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable RedundantJumpStatement
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public interface IFieldControl
{
    ComponentField  ComponentField  { get; init; }
}

public class ComponentField
{
    internal            bool            IsLabeled { get; init; } = true;

#region internal fields
    internal readonly   string          name;
    internal            Control         control;
    private             FieldData       data;
    private  readonly   ComponentType   componentType;
    private  readonly   Type            type;
    private  readonly   int             index;
    private  readonly   Var.Member      member;

    public   override   string          ToString() => $"'{name}', type: {type.Name}";

    #endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    private ComponentField(string name, ComponentType componentType, Type type, int index, Var.Member member) {
        this.name           = name;
        this.member         = member;
        this.componentType  = componentType;
        this.type           = type;
        this.index          = index;
    }
    
#region create component fields
    private static bool AddComponentFields(
        List<ComponentField>    fields,
        ComponentType           componentType,
        Type                    type,
        string                  fieldName,
        Var.Member              member)
    {
        if (type == typeof(Position) ||
            type == typeof(Scale3))
        {
            fieldName     ??= "value";
            var field       = new ComponentField(fieldName,    componentType, type,   0, member);
            field.control   = new Vector3Field { ComponentField = field };
            fields.Add(field);
            return true;
        }
        if (type == typeof(Transform)) {
            var field0      = new ComponentField("position",   componentType, type,  0, member);
            var field1      = new ComponentField("rotation",   componentType, type,  1, member);
            field0.control  = new Vector3Field { ComponentField = field0 };
            field1.control  = new Vector3Field { ComponentField = field1 };
            fields.Add(field0);
            fields.Add(field1);
            return true;
        }
        if (type == typeof(EntityName)) {
            fieldName     ??= nameof(EntityName.value);
            var field       = new ComponentField(fieldName,    componentType, type, 0, member);
            field.control   = new StringField { ComponentField = field };
            fields.Add(field);
            return true;
        }
        if (type == typeof(Unresolved)) {
            fieldName     ??= "unresolved";
            var field       = new ComponentField(fieldName,    componentType, type, 0, member) { IsLabeled = false };
            field.control   = new UnresolvedField{ ComponentField = field };
            fields.Add(field);
            return true;
        }
        return false;
    }
    
    internal static void AddComponentTypeFields(List<ComponentField> fields, ComponentType componentType)
    {
        var type    = componentType.Type;
        if (AddComponentFields(fields, componentType, type, null, default)) {
            return;
        }
        // add custom struct component fields
        AddScriptFields(fields, componentType, type);
    }
        
    internal static void AddScriptFields(List<ComponentField> fields, ComponentType componentType, Type type)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var propFields  = classMapper.PropFields.fields;
        for (int n = 0; n < propFields.Length; n++)
        {
            var propField   = propFields[n];
            var fieldType   = propField.fieldType.type;
            var member      = classMapper.GetMember(propField.name);
            if (AddComponentFields(fields, componentType, fieldType, propField.name, member)) {
                continue;
            }
            var field       = new ComponentField(propField.name, componentType, fieldType, 0, member);
            field.control   = CreateField(fieldType, field);
            fields.Add(field);
        }
    }
    
    private static Control CreateField (Type fieldType, ComponentField field)
    {
        if (fieldType == typeof(string)) {
            return new StringField  { ComponentField = field };
        }
        if (fieldType == typeof(int)) {
            return new ValueField   { ComponentField = field };
        } else {
            return new ValueField   { ComponentField = field };
        }
    }
    #endregion
    
#region read component values
    internal static void SetComponentFields(ComponentField[] componentFields, Entity entity, IComponent component)
    {
        foreach (var field in componentFields) {
            field.data  = default; // clear to prevent update calls
            FieldData data;
            if (field.member == null) {
                data = new FieldData(entity, component);    
            } else {
                data = new FieldData(entity, component, field.member);
            }
            SetComponentField(field, data);
            field.data  = data;
        }
    }
    
    internal static void SetScriptFields(Entity entity, ComponentField[] componentFields, Script script)
    {
        foreach (var field in componentFields) {
            field.data  = default; // clear to prevent update calls
            var data    = new FieldData(entity, script, field.member);
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
        if (type == typeof(Scale3)) {
            var control     = (Vector3Field)field.control; 
            var scale3      = (Scale3)data.GetData();
            control.X       = scale3.x;
            control.Y       = scale3.y;
            control.Z       = scale3.z;
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
        if (type == typeof(Unresolved)) {
            var control     = (UnresolvedField)field.control;
            var unresolved  = (Unresolved)data.GetData();
            control.Set(unresolved);
            return;
        }
        if (type == typeof(EntityName)) {
            var control     = (StringField)field.control;
            var entityName  = (EntityName)data.GetData();
            control.InitValue(entityName.value);
            return;
        }
        if (type == typeof(string)) {
            var control     = (StringField)field.control;
            var value       = field.member.GetVar(data.instance).String;
            control.InitValue(value);
            return;
        }
        if (type == typeof(int)) {
            var control     = (ValueField)field.control;
            control.Value   = field.member.GetVar(data.instance).Int32.ToString();
            return;
        }
        if (type == typeof(float)) {
            var control     = (ValueField)field.control;
            control.Value   = field.member.GetVar(data.instance).Flt32.ToString(CultureInfo.InvariantCulture);
            return;
        }
        throw new InvalidOperationException($"missing field assignment. field: {field.name}, type: {type.Name}");
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
            case ComponentMember:
                SetScriptVector(data.instance, vector);
                EntityUtils.AddEntityComponentValue(data.entity, componentType, data.instance);
                return;
            case ScriptMember:
                SetScriptVector(data.instance, vector);
                EntityUtils.AddEntityScript(data.entity, (Script)data.instance);
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
                    break;
                case 1:
                    transform.m21 = vector.X;
                    transform.m22 = vector.Y;
                    transform.m23 = vector.Z;
                    break;
            }
            entity.AddComponent(transform); // send ComponentChangedArgs event to update other editor controls
            return;
        }
        if (type == typeof(Position))
        {
            // ref var position = ref entity.GetComponent<Position>();
            // position.value = vector;
            var position = new Position { value = vector };
            entity.AddComponent(position); // send ComponentChangedArgs event to update other editor controls
            return;
        }
        if (type == typeof(Scale3))
        {
            // ref var position = ref entity.GetComponent<Scale3>();
            // position.value = vector;
            var scale3 = new Scale3 { value = vector };
            entity.AddComponent(scale3); // send ComponentChangedArgs event to update other editor controls
            return;
        }
        throw new NotImplementedException($"SetComponentVector() type: {type.Name}");
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
            var var = new Var(new Position { value = vector });
            member.SetVar(script, var);
            return;
        }
        if (type == typeof(Scale3))
        {
            var var = new Var(new Scale3 { value = vector });
            member.SetVar(script, var);
            return;
        }
        throw new NotImplementedException($"SetScriptVector() type: {type.Name}");
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
            case ComponentMember:
                SetScriptString(data.instance, value);
                EntityUtils.AddEntityComponentValue(data.entity, componentType, data.instance);
                return;
            case ScriptMember:
                SetScriptString(data.instance, value);
                EntityUtils.AddEntityScript(data.entity, (Script)data.instance);
                return;
        }
    }
    
    private void SetComponentString(Entity entity, string value)
    {
        if (type == typeof(EntityName)) {
            entity.AddComponent(new EntityName(value));
            return;
        }
        if (type == typeof(float)) {
            _ = 1;
            // todo
        }
        throw new NotImplementedException($"SetComponentString() type: {type.Name}");
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
        if (type == typeof(float)) {
            var f32     = float.Parse(value);
            member.SetVar(script, new Var(f32));
            return;
        }
        throw new NotImplementedException($"SetScriptString() type: {type.Name}");
    }
    #endregion
}
