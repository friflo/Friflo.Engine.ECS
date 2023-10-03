// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Mapper;
using static Friflo.Fliox.Engine.ECS.StructUtils;

// Hard rule: this file/section MUST NOT use GameEntity instances

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertConstructorToMemberInitializers
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    private Archetype GetArchetypeWith<T>(Archetype current)
        where T : struct
    {
        var hash = GetHashWith(typeof(T), current);
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var config  = GetArchetypeConfig();
        var newHeap = StructHeap<T>.Create(config);
        archetype   = Archetype.CreateFromArchetype(config, current, newHeap);
        AddArchetype(archetype);
        return archetype;
    }
    
    private Archetype GetArchetypeWith(Archetype current, ComponentFactory factory)
    {
        var hash = factory.structHash ^ current.typeHash;
        if (TryGetArchetype(hash, out var archetype)) {
            return archetype;
        }
        var newHeap = factory.CreateHeap(Static.DefaultCapacity);
        var config  = GetArchetypeConfig();
        archetype   = Archetype.CreateFromArchetype(config, current, newHeap);
        AddArchetype(archetype);
        return archetype;
    }
    
    private Archetype GetArchetype(ComponentFactory factory)
    {
        // could perform lookup per lookup array
        if (TryGetArchetype(factory.structHash, out var archetype)) {
            return archetype;
        }
        var newHeap = factory.CreateHeap(Static.DefaultCapacity);
        var config  = GetArchetypeConfig();
        archetype   = Archetype.CreateFromArchetype(config, defaultArchetype, newHeap);
        AddArchetype(archetype);
        return archetype;
    }
    
    private Archetype GetArchetypeWithout(Archetype archetype, Type removeType)
    {
        var hash = GetHashWithout(removeType, archetype);
        if (TryGetArchetype(hash, out var result)) {
            return result;
        }
        var componentCount = archetype.Heaps.Length - 1;
        if (componentCount == 0) {
            return null;
        }
        var heaps = new StructHeap[componentCount];
        int n = 0;
        foreach (var heap in archetype.Heaps) {
            if (heap.type == removeType)
                continue;
            heaps[n++] = heap;
        }
        if (n != componentCount) {
            throw new InvalidOperationException("unexpected length");
        }
        var config  = GetArchetypeConfig();
        result      = Archetype.CreateWithHeaps(config, heaps);
        AddArchetype(result);
        return result;
    }
    
    internal bool TryGetArchetype (long hash, out Archetype result) {
        foreach (var arch in archetypeInfos) {
            if (arch.hash != hash) {
                continue;
            }
            result = arch.type;
            return true;
        }
        result = null;
        return false;
    }
    
    internal void AddArchetype (Archetype archetype) {
        if (archetype == null) {
            return;
        }
        if (archetypesCount == archetypes.Length) {
            var newLen = 2 * archetypes.Length;
            Utils.Resize(ref archetypes,     newLen);
            Utils.Resize(ref archetypeInfos, newLen);
        }
        if (archetype.archIndex != archetypesCount) {
            throw new InvalidOperationException("invalid archIndex");
        }
        archetypes    [archetypesCount] = archetype;
        archetypeInfos[archetypesCount] = new ArchetypeInfo(archetype.typeHash, archetype);
        archetypesCount++;
    }
    
    // ------------------------------------ hash utils ------------------------------------
    private static long GetHashWith(Type newType, Archetype archetype) {
        return newType.Handle() ^ archetype.typeHash;
    }
    
    private static long GetHashWithout(Type removeType, Archetype archetype)
    {
        return removeType.Handle() ^ archetype.typeHash;
    }
    
    private static long GetHash(ReadOnlySpan<long> hashes) {
        long hash = 0;
        foreach (var h in hashes) {
            hash ^= h;
        }
        return hash;
    }
    
    internal static long GetHash(StructHeap[] heaps, StructHeap newComp) {
        long hash = default;
        if (newComp != null) {
            hash = newComp.hash;
        }
        foreach (var heap in heaps) {
            hash ^= heap.hash;
        }
        return hash;
    }
    
    // ------------------------------------ entity / component management ------------------------------------
    internal bool AddComponent<T>(
            int                 id,
        ref Archetype           archetype,
        ref int                 compIndex,
        in  T                   component,
            ComponentUpdater    updater)
        where T : struct
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
            compIndex           = arch.MoveEntityTo(id, compIndex, newArchetype, updater);
            archetype           = arch = newArchetype;
        } else {
            // --- add entity to archetype
            arch                = GetArchetype<T>();
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
        ref Archetype           archetype,
        ref int                 compIndex,
            ComponentUpdater    updater)
        where T : struct
    {
        var arch = archetype;
        if (arch == null) {
            return false;
        }
        var heap = arch.heapMap[StructHeap<T>.StructIndex];
        if (heap == null) {
            return false;
        }
        var newArchetype = GetArchetypeWithout(arch, typeof(T));
        if (newArchetype == null) {
            int removePos = compIndex; 
            // --- update entity
            compIndex   = 0;
            archetype   = null;
            arch.MoveLastComponentsTo(removePos, updater);
            return true;
        }
        // --- change entity archetype
        compIndex   = arch.MoveEntityTo(id, compIndex, newArchetype, updater);
        archetype   = newArchetype;
        return true;
    }
    
    public void RegisterStructComponent<T>() where T : struct  {
        var structIndex         = StructHeap<T>.StructIndex;
        var structKey           = StructHeap<T>.StructKey;
        var factory             = new StructFactory<T>(structIndex, structKey, typeStore);
        factories[structKey]    = factory;
    }
    
    public void RegisterClassComponent<T>() where T : ClassComponent  {
        var classKey            = ClassType<T>.ClassKey;
        var factory             = new ClassFactory<T>(typeStore);
        factories[classKey]     = factory;
    }
    
    internal bool ReadStructComponent(
        ObjectReader        reader,
        JsonValue           json,
        int                 id,
        ref Archetype       archetype,
        ref int             compIndex,
        ComponentFactory    factory,
        ComponentUpdater    updater)
    {
        var arch = archetype;
        if (arch != defaultArchetype) {
            var structHeap = arch.heapMap[factory.structIndex];
            if (structHeap != null) {
                // --- change component value 
                structHeap.Read(reader, compIndex, json);
                return false;
            }
            // --- change entity archetype
            var newArchetype    = archetype.store.GetArchetypeWith(archetype, factory);
            compIndex           = arch.MoveEntityTo(id, compIndex, newArchetype, updater);
            archetype           = arch = newArchetype;
        } else {
            // --- add entity to archetype
            arch                = archetype.store.GetArchetype(factory);
            compIndex           = arch.AddEntity(id);
            archetype           = arch;
        }
        // --- set component value 
        var heap = arch.heapMap[factory.structIndex];
        heap.Read(reader, compIndex, json);
        return true;
    }
}

