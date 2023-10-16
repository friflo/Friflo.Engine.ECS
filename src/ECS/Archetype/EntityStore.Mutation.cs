// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// Hard rule: this file MUST NOT access GameEntity's

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public partial class EntityStore
{
    private Archetype GetArchetypeWith<T>(Archetype current)
        where T : struct, IStructComponent
    {
        searchKey.SetWith(current, StructHeap<T>.StructIndex);
        if (archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config          = GetArchetypeConfig();
        var schema          = Static.ComponentSchema;
        var heaps           = current.Heaps;
        var componentCount  = heaps.Length;
        var types           = new List<ComponentType>(componentCount + 1);
        for (int n = 0; n < componentCount; n++) {
            var heap = heaps[n];
            types.Add(schema.GetStructType(heap.structIndex, heap.type));
        }
        types.Add(schema.GetStructType(StructHeap<T>.StructIndex, typeof(T)));
        var archetype = Archetype.CreateWithStructTypes(config, types, current.tags);
        AddArchetype(archetype);
        return archetype;
    }
    
    private Archetype GetArchetypeWithout(Archetype archetype, int structIndex, Type removeType)
    {
        searchKey.SetWithout(archetype, structIndex);
        if (archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var heaps           = archetype.Heaps;
        var componentCount  = heaps.Length - 1;
        var types           = new List<ComponentType>(componentCount);
        var config          = GetArchetypeConfig();
        var schema          = Static.ComponentSchema;
        foreach (var heap in heaps) {
            if (heap.type == removeType)
                continue;
            types.Add(schema.GetStructType(heap.structIndex, heap.type));
        }
        var result = Archetype.CreateWithStructTypes(config, types, archetype.tags);
        AddArchetype(result);
        return result;
    }
    
    private Archetype GetArchetypeWithTags(Archetype archetype, in Tags tags)
    {
        var heaps           = archetype.Heaps;
        var types           = new List<ComponentType>(heaps.Length);
        var config          = GetArchetypeConfig();
        var schema          = Static.ComponentSchema;
        foreach (var heap in heaps) {
            types.Add(schema.GetStructType(heap.structIndex, heap.type));
        }
        var result = Archetype.CreateWithStructTypes(config, types, tags);
        AddArchetype(result);
        return result;
    }
    
    internal void AddArchetype (Archetype archetype)
    {
        if (archsCount == archs.Length) {
            var newLen = 2 * archs.Length;
            Utils.Resize(ref archs,     newLen);
        }
        if (archetype.archIndex != archsCount) {
            throw new InvalidOperationException($"invalid archIndex. expect: {archsCount}, was: {archetype.archIndex}");
        }
        archs[archsCount] = archetype;
        archsCount++;
        archSet.Add(archetype.key);
    }
    
    // ------------------------------------ add / remove struct component ------------------------------------
    internal bool AddComponent<T>(
            int                 id,
        ref Archetype           archetype,  // possible mutation is not null
        ref int                 compIndex,
        in  T                   component)
        where T : struct, IStructComponent
    {
        var arch = archetype;
        if (arch != defaultArchetype) {
            var structHeap = arch.heapMap[StructHeap<T>.StructIndex];
            if (structHeap != null) {
                // --- change component value 
                var heap = (StructHeap<T>)structHeap;
                heap.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize] = component;
                return false;
            }
            // --- change entity archetype
            var newArchetype    = GetArchetypeWith<T>(arch);
            compIndex           = arch.MoveEntityTo(id, compIndex, newArchetype);
            archetype           = arch = newArchetype;
        } else {
            // --- add entity to archetype
            arch                = GetArchetype<T>(arch.tags);
            compIndex           = arch.AddEntity(id);
            archetype           = arch;
        }
        // --- set component value
        var heap2 = (StructHeap<T>)arch.heapMap[StructHeap<T>.StructIndex];
        heap2.chunks[compIndex / ChunkSize].components[compIndex % ChunkSize] = component;
        return true;
    }
    
    internal bool RemoveComponent<T>(
            int                 id,
        ref Archetype           archetype,    // possible mutation is not null
        ref int                 compIndex)
        where T : struct, IStructComponent
    {
        var arch = archetype;
        var heap = arch.heapMap[StructHeap<T>.StructIndex];
        if (heap == null) {
            return false;
        }
        var newArchetype    = GetArchetypeWithout(arch, StructHeap<T>.StructIndex, typeof(T));
        if (newArchetype == defaultArchetype) {
            int removePos = compIndex; 
            // --- update entity
            archetype       = defaultArchetype;
            compIndex       = 0;
            arch.MoveLastComponentsTo(removePos);
            return true;
        }
        // --- change entity archetype
        archetype       = newArchetype;
        compIndex       = arch.MoveEntityTo(id, compIndex, newArchetype);
        return true;
    }
    
    // ------------------------------------ add / remove entity Tag ------------------------------------
    internal bool AddTags(
        in Tags             tags,
        int                 id,
        ref Archetype       archetype,      // possible mutation is not null
        ref int             compIndex)
    {
        var arch            = archetype;
        var archTagsValue   = arch.tags.bitSet.value;
        var tagsValue       = tags.bitSet.value;
        if (archTagsValue == tagsValue) {
            return false;
        } 
        searchKey.structs           = arch.structs;
        searchKey.tags.bitSet.value = archTagsValue | tagsValue;
        searchKey.CalculateHashCode();
        Archetype newArchetype;
        if (archSet.TryGetValue(searchKey, out var archetypeKey)) {
            newArchetype = archetypeKey.archetype;
        } else {
            newArchetype = GetArchetypeWithTags(arch, searchKey.tags);
        }
        if (arch != defaultArchetype) {
            compIndex   = arch.MoveEntityTo(id, compIndex, newArchetype);
            archetype   = newArchetype;
            return true;
        }
        compIndex           = newArchetype.AddEntity(id);
        archetype           = newArchetype;
        return true;
    }
    
    internal bool RemoveTags(
        in Tags             tags,
        int                 id,
        ref Archetype       archetype,      // possible mutation is not null
        ref int             compIndex)
    {
        var arch            = archetype;
        var archTags        = arch.tags.bitSet.value;
        var archTagsRemoved = archTags & ~tags.bitSet.value;
        if (archTagsRemoved == archTags) {
            return false;
        }
        searchKey.structs           = arch.structs;
        searchKey.tags.bitSet.value = archTagsRemoved;
        searchKey.CalculateHashCode();
        Archetype newArchetype;
        if (archSet.TryGetValue(searchKey, out var archetypeKey)) {
            newArchetype = archetypeKey.archetype;
        } else {
            newArchetype = GetArchetypeWithTags(arch, searchKey.tags);
        }
        if (newArchetype == defaultArchetype) {
            int removePos = compIndex; 
            // --- update entity
            compIndex   = 0;
            archetype   = defaultArchetype;
            arch.MoveLastComponentsTo(removePos);
            return true;
        }
        compIndex   = arch.MoveEntityTo(id, compIndex, newArchetype);
        archetype   = newArchetype;
        return true;
    }
}
