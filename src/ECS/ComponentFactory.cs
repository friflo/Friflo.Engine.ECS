// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

namespace Friflo.Fliox.Engine.ECS;

internal abstract class ComponentFactory
{
    internal readonly   int     structIndex;
    internal readonly   string  structKey;
    internal readonly   long    structHash;
        
    internal abstract   StructHeap  CreateHeap          (int capacity);
    internal abstract   void        ReadClassComponent  (ObjectReader reader, JsonValue json, GameEntity entity);
    internal abstract   bool        IsStructFactory     { get; }
    
    internal ComponentFactory(int structIndex, string structKey, long structHash) {
        this.structIndex    = structIndex;
        this.structKey      = structKey;
        this.structHash     = structHash;
    }
}

internal sealed class StructFactory<T> : ComponentFactory 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"StructFactory: {typeof(T).Name}";

    internal StructFactory(int structIndex, string structKey, TypeStore typeStore)
        : base(structIndex, structKey, typeof(T).Handle())
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   bool    IsStructFactory => true;
    internal override   void    ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity)
        => throw new InvalidOperationException("operates only on ClassFactory<>");
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(structIndex, structKey, capacity, typeMapper);   
    }
}

internal sealed class ClassFactory<T> : ComponentFactory 
    where T : ClassComponent
{
    private readonly    TypeMapper<T>   typeMapper;
    public  override    string          ToString() => $"ClassFactory: {typeof(T).Name}";
    
    internal ClassFactory(TypeStore typeStore)
        : base(-1, null, 0)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   bool        IsStructFactory => false;
    internal override   StructHeap  CreateHeap(int capacity)
        => throw new InvalidOperationException("operates only on StructFactory<>");
    
    internal override void ReadClassComponent(ObjectReader reader, JsonValue json, GameEntity entity) {
        var classComponent = entity.GetClassComponent<T>();
        if (classComponent != null) { 
            reader.ReadToMapper(typeMapper, json, classComponent, true);
            return;
        }
        classComponent = reader.ReadMapper(typeMapper, json);
        entity.AddClassComponent(classComponent);
    }
}