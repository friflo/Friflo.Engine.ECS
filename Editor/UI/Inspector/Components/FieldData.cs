// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Friflo.Json.Fliox.Mapper.Map;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

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
