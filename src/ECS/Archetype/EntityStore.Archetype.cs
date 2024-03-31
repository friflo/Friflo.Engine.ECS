// ﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard rule: this file MUST NOT use type: Entity

using System;
using Friflo.Engine.ECS.Utils;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    // -------------------------------------- get archetype --------------------------------------
#region get archetype
    internal static ArchetypeConfig GetArchetypeConfig(EntityStoreBase store) {
        return new ArchetypeConfig (store, store.archsCount);
    }
    
    /// <summary>
    /// Return the <see cref="Archetype"/> storing the specified <paramref name="componentTypes"/> and <paramref name="tags"/>.<br/>
    /// The <see cref="Archetype"/> is created if not already present.
    /// </summary>
    public Archetype GetArchetype(in ComponentTypes componentTypes, in Tags tags = default)
    {
        searchKey.componentTypes    = componentTypes;
        searchKey.tags              = tags;
        searchKey.CalculateHashCode();
        if (archSet.TryGetValue(searchKey, out var key)) {
            return key.archetype;
        }
        var config      = GetArchetypeConfig(this);
        var archetype   = Archetype.CreateWithComponentTypes(config, componentTypes, tags);
        AddArchetype(this, archetype);
        return archetype;
    }
    
    /// <summary>
    /// Return the <see cref="Archetype"/> storing the specified <paramref name="tags"/>.<br/>
    /// The <see cref="Archetype"/> is created if not already present.
    /// </summary>
    public Archetype GetArchetype(in Tags tags) => GetArchetype(default, tags);
    
    /// <summary>
    /// Return the <see cref="Archetype"/> storing the specified <paramref name="componentTypes"/> and <paramref name="tags"/>.<br/>
    /// </summary>
    /// <returns> null if the <see cref="Archetype"/> is not present. </returns>
    public Archetype FindArchetype(in ComponentTypes componentTypes, in Tags tags) {
        searchKey.componentTypes    = componentTypes;
        searchKey.tags              = tags;
        searchKey.CalculateHashCode();
        archSet.TryGetValue(searchKey, out var actualValue);
        return actualValue?.archetype;
    }
    #endregion
    
#region get / add archetype
    internal bool TryGetValue(ArchetypeKey searchKey, out ArchetypeKey archetypeKey) {
        return archSet.TryGetValue(searchKey, out archetypeKey);
    }
        
    private static Archetype GetArchetypeWith(EntityStoreBase store, Archetype current, int structIndex)
    {
        var searchKey = store.searchKey;
        searchKey.SetWith(current, structIndex);
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config          = GetArchetypeConfig(store);
        var componentTypes  = current.componentTypes;
        componentTypes.bitSet.SetBit(structIndex);
        var archetype = Archetype.CreateWithComponentTypes(config, componentTypes, current.tags);
        AddArchetype(store, archetype);
        return archetype;
    }
    
    private static Archetype GetArchetypeWithout(EntityStoreBase store, Archetype archetype, int structIndex)
    {
        var searchKey = store.searchKey;
        searchKey.SetWithout(archetype, structIndex);
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config          = GetArchetypeConfig(store);
        var componentTypes  = archetype.componentTypes;
        componentTypes.bitSet.ClearBit(structIndex);
        var result = Archetype.CreateWithComponentTypes(config, componentTypes, archetype.tags);
        AddArchetype(store, result);
        return result;
    }
    
    private static Archetype GetArchetypeWithTags(EntityStoreBase store, Archetype archetype, in Tags tags)
    {
        var searchKey               = store.searchKey;
        searchKey.componentTypes    = archetype.componentTypes;
        searchKey.tags              = tags;
        searchKey.CalculateHashCode();
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config  = GetArchetypeConfig(store);
        var result  = Archetype.CreateWithComponentTypes(config, archetype.componentTypes, tags);
        AddArchetype(store, result);
        return result;
    }
    
    internal Archetype GetArchetypeAdd(Archetype type, Span<int> addComponents, in Tags addTags)
    {
        var key = searchKey;
        key.tags.bitSet             = BitSet.Add(type.tags.bitSet, addTags.bitSet);
        key.componentTypes.bitSet   = type.componentTypes.bitSet;
        foreach (var structIndex in addComponents) {
            key.componentTypes.bitSet.SetBit(structIndex);
        }
        key.CalculateHashCode();
        if (archSet.TryGetValue(key, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config      = GetArchetypeConfig(this);
        var archetype   = Archetype.CreateWithComponentTypes(config, key.componentTypes, key.tags);
        AddArchetype(this, archetype);
        return archetype;
    }
    
    internal Archetype GetArchetypeRemove(Archetype type, Span<int> removeComponents, in Tags removeTags)
    {
        var key = searchKey;
        key.tags.bitSet             = BitSet.Remove(type.tags.bitSet, removeTags.bitSet);
        key.componentTypes.bitSet   = type.componentTypes.bitSet;
        foreach (var structIndex in removeComponents) {
            key.componentTypes.bitSet.ClearBit(structIndex);
        }
        key.CalculateHashCode();
        if (archSet.TryGetValue(key, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config      = GetArchetypeConfig(this);
        var archetype   = Archetype.CreateWithComponentTypes(config, key.componentTypes, key.tags);
        AddArchetype(this, archetype);
        return archetype;
    }
    
    internal static void AddArchetype (EntityStoreBase store, Archetype archetype)
    {
        if (store.archsCount == store.archs.Length) {
            var newLen = 2 * store.archs.Length;
            ArrayUtils.Resize(ref store.archs,     newLen);
        }
        if (archetype.archIndex != store.archsCount) {
            throw new InvalidOperationException($"invalid archIndex. expect: {store.archsCount}, was: {archetype.archIndex}");
        }
        store.archs[store.archsCount] = archetype;
        store.archsCount++;
        store.archSet.Add(archetype.key);
    }
    #endregion
}
