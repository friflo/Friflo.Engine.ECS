// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;

namespace Friflo.Fliox.Engine.Client;

/// <summary>
/// Create the <see cref="JsonValue"/> from all class / struct components used at <see cref="DataNode.components"/>.<br/>
/// </summary>
internal sealed class ComponentWriter
{
    private readonly ObjectWriter writer;
    
    internal static readonly ComponentWriter Instance = new ComponentWriter();
    
    private ComponentWriter() {
        writer = new ObjectWriter(EntityStore.Static.TypeStore);
    }
    
    internal JsonValue  Write(GameEntity entity)
    {
        var archetype = entity.archetype;
        if (entity.ComponentCount == 0) {
            return default;
        }
        var heaps = archetype.Heaps;
        for (int n = 0; n < heaps.Length; n++) {
            var heap = heaps[n];
            heap.Write(writer, entity.compIndex);
        }
        
        var classComponents = entity.ClassComponents;
        if (classComponents != null) {
            
        }
        return default;
    }
    
}