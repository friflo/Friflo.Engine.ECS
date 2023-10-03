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
    internal readonly   string  classKey;
    internal readonly   long    structHash;
        
    internal abstract   StructHeap  CreateHeap          (int capacity);
    internal abstract   object      ReadClassComponent  (ObjectReader reader, JsonValue json);
    internal abstract   bool        IsStructFactory     { get; }
    
    internal ComponentFactory(int structIndex, string structKey, long structHash, string  classKey) {
        this.structIndex    = structIndex;
        this.structKey      = structKey;
        this.structHash     = structHash;
        this.classKey       = classKey;
    }
}

internal sealed class StructFactory<T> : ComponentFactory 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    
    internal StructFactory(int structIndex, string structKey, TypeStore typeStore)
        : base(structIndex, structKey, typeof(T).Handle(), null)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   bool    IsStructFactory => true;
    internal override   object  ReadClassComponent(ObjectReader reader, JsonValue json) => throw new InvalidOperationException("operates only on ClassFactory<>");
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(structIndex, structKey, capacity, typeMapper);   
    }
}

internal sealed class ClassFactory<T> : ComponentFactory 
    where T : class
{
    private readonly    TypeMapper<T>   typeMapper;
    
    internal ClassFactory(string classKey, TypeStore typeStore)
        : base(-1, null, 0, classKey)
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override   bool        IsStructFactory => false;
    internal override   StructHeap  CreateHeap(int capacity) => throw new InvalidOperationException("operates only on StructFactory<>");
    
    internal override object ReadClassComponent(ObjectReader reader, JsonValue json) {
        return reader.ReadMapper(typeMapper, json);
    }
}