// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
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
#endregion
    
    private static readonly TypeStore TypeStore = new TypeStore(); // todo  use shared TypeStore
    
    
    internal static ComponentField[] GetComponentFields(Type type)
    {
        var classMapper = TypeStore.GetTypeMapper(type);
        var fields      = classMapper.PropFields.fields;
        var result      = new ComponentField[fields.Length];
        
        for (int n = 0; n < fields.Length; n++) {
            var propField   = fields[n];
            var member      = classMapper.GetMember(propField.name);
            result[n]       = new ComponentField { field = propField, member = member };
        }
        return result;
    }
}
