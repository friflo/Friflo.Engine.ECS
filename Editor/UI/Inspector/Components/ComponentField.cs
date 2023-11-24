// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
    
    internal static void AddFields(List<ComponentField> componentField, Type type, object instance, string componentName)
    {
        if (type == typeof(Position)) {
            var position    = (Position)instance;
            var control     = new Vector3Field { vector = position.value };
            var field       = new ComponentField { name = componentName, control = control };
            componentField.Add(field);
            return;
        }
        if (type == typeof(EntityName)) {
            var name        = (EntityName)instance;
            var control     = new StringField { Value = name.value };
            var field       = new ComponentField { name = componentName, control = control };
            componentField.Add(field);
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
            var field       = new ComponentField { name = propField.name, control = control };
            componentField.Add(field);
        }
    }
}
