// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

internal struct ComponentField
{
#region internal fields
    internal    string      name;
    internal    Control     control;
#endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    private ComponentField(string name, Control control) {
        this.name       = name;
        this.control    = control;
    }
    
    internal static void AddComponentFields(
        List<ComponentField>    componentField,
        Type                    type,
        object                  instance,
        string                  fieldName)
    {
        if (type == typeof(Position)) {
            var position    = (Position)instance;
            fieldName     ??= "Value";
            componentField.Add(new ComponentField(fieldName,         new Vector3Field { vector = position.value }));
            return;
        }
        if (type == typeof(Transform)) {
            var t           = (Transform)instance;
            var position    = new Vector3(t.m11, t.m12, t.m13);
            var rotation    = new Vector3(t.m21, t.m22, t.m23);
            componentField.Add(new ComponentField("Position",   new Vector3Field { vector = position }));
            componentField.Add(new ComponentField("Rotation",   new Vector3Field { vector = rotation }));
            return;
        }
        if (type == typeof(EntityName)) {
            var name        = (EntityName)instance;
            var control     = new StringField { Value = name.value };
            fieldName     ??= "Value";
            componentField.Add(new ComponentField(fieldName, control));
            return;
        }
        var classMapper     = TypeStore.GetTypeMapper(type);
        var fields          = classMapper.PropFields.fields;
        for (int n = 0; n < fields.Length; n++) {
            var propField   = fields[n];
            var member      = classMapper.GetMember(propField.name);
            var value       = member.GetVar(instance);
            var str         = value.AsString();
            var control     = new StringField { Value = str };
            var field       = new ComponentField(propField.name, control);
            componentField.Add(field);
        }
    }
}
