// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Numerics;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

internal struct ComponentField
{
#region internal fields
    internal    PropField   field;
    /// <summary>Access member value with <see cref="Var.Member.GetVar"/></summary>
    internal    Var.Member  member;
    
    internal    Control     control;
#endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    
    internal static ComponentField[] GetComponentFields(Type type, object instance)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var fields      = classMapper.PropFields.fields;
        var result      = new ComponentField[fields.Length];
        
        for (int n = 0; n < fields.Length; n++) {
            var propField   = fields[n];
            var member      = classMapper.GetMember(propField.name);
            var control     = CreateControl(type, member, instance);
            result[n]       = new ComponentField { field = propField, member = member, control = control };
        }
        return result;
    }
    
    private static Control CreateControl(Type type, Var.Member member, object instance)
    {
        if (type == typeof(Position)) {
            var position = (Position)instance;
            return new Vector3Field { vector = position.value };
        }
        if (type == typeof(EntityName)) {
            var name = (EntityName)instance;
            return new StringField { Value = name.value };
        }
        var value   = member.GetVar(instance);
        var str     = value.AsString();
        return new StringField { Value = str };
    }
}
