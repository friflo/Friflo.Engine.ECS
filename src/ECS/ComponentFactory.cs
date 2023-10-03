// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Json.Fliox.Mapper;
using Friflo.Json.Fliox.Mapper.Map;

namespace Friflo.Fliox.Engine.ECS;

internal abstract class ComponentFactory
{
    internal readonly   int     structIndex;
    internal readonly   string  structKey;
    internal readonly   long    typeHash;
        
    internal abstract StructHeap CreateHeap(int capacity);
    
    internal ComponentFactory(int structIndex, string structKey, long typeHash) {
        this.structIndex    = structIndex;
        this.structKey      = structKey;
        this.typeHash       = typeHash;
    }
}

internal sealed class ComponentFactory<T> : ComponentFactory 
    where T : struct
{
    private readonly    TypeMapper<T>   typeMapper;
    
    internal ComponentFactory(int structIndex, string structKey, TypeStore typeStore)
        : base(structIndex, structKey, typeof(T).Handle())
    {
        typeMapper = typeStore.GetTypeMapper<T>();
    }
    
    internal override StructHeap CreateHeap(int capacity) {
        return new StructHeap<T>(structIndex, structKey, capacity, typeMapper);   
    }
}