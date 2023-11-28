// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;
using static Friflo.Fliox.Engine.ECS.SchemaTypeKind;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class ComponentType : SchemaType
{
    /// <summary>
    /// The index in <see cref="EntitySchema.Components"/>.<br/>
    /// </summary>
    public   readonly   int             structIndex;    //  4
    
    internal abstract StructHeap    CreateHeap();
    internal abstract bool          AddEntityComponent(Entity entity);
    
    protected ComponentType(string componentKey, int structIndex, Type type)
        : base (componentKey, type, Component)
    {
        this.structIndex = structIndex;
    }
}

internal sealed class ComponentType<T> : ComponentType 
    where T : struct, IComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"component: '{componentKey}' [{typeof(T).Name}]";

    internal ComponentType(string componentKey, int structIndex, TypeStore typeStore)
        : base(componentKey, structIndex, typeof(T))
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override bool AddEntityComponent(Entity entity) {
        var store   = entity.archetype.entityStore;
        var result  = store.AddComponent<T>(entity.id, structIndex, ref entity.archetype, ref entity.compIndex, default);
        // send event
        EntityStore.SendAddedComponent(store, entity.id, structIndex);
        return result;
    }
    
    internal override StructHeap CreateHeap() {
        return new StructHeap<T>(structIndex, typeMapper);
    }
}