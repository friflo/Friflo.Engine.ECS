// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard rule: this file MUST NOT use type: Entity

using System;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    internal void AddArchetypeCapacity(int capacity) {
        internBase.archetypesCapacity += capacity;
        shrinkArchetypes = internBase.archetypesCapacity > internBase.shrinkRatio * entityCount;
    }
    
    private void SetShrinkRatio(double value) {
        internBase.shrinkRatio = value;
        AddArchetypeCapacity(0);
    }
    
    // -------------------------------------- get archetype --------------------------------------
#region get archetype
    internal static ArchetypeConfig GetArchetypeConfig(EntityStoreBase store) {
        return new ArchetypeConfig (store, store.archsCount, Static.EntitySchema);
    }
    
    /// <summary>
    /// Return the <see cref="Archetype"/> storing the specified <paramref name="componentTypes"/> and <paramref name="tags"/>.<br/>
    /// The <see cref="Archetype"/> is created if not already present.
    /// </summary>
    public Archetype GetArchetype(in ComponentTypes componentTypes, in Tags tags = default)
    {
        var key             = searchKey;
        key.componentTypes  = componentTypes;
        key.tags            = tags;
        key.CalculateHashCode();
        if (archSet.TryGetValue(key, out var archetypeKey)) {
            return archetypeKey.archetype;
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
    public Archetype FindArchetype(in ComponentTypes componentTypes, in Tags tags)
    {
        var key             = searchKey;
        key.componentTypes  = componentTypes;
        key.tags            = tags;
        key.CalculateHashCode();
        archSet.TryGetValue(key, out var actualValue);
        return actualValue?.archetype;
    }
    #endregion
    
#region get / add archetype
    internal bool TryGetValue(ArchetypeKey searchKey, out ArchetypeKey archetypeKey) {
        return archSet.TryGetValue(searchKey, out archetypeKey);
    }
        
    internal static Archetype GetArchetypeWith(EntityStoreBase store, Archetype current, int structIndex)
    {
        var typeIndex = current.componentAdd.FindType(structIndex);
        if (typeIndex >= 0) {
            return current.store.archs[typeIndex];
        }
        var key = store.searchKey;
        key.SetWith(current, structIndex);
        if (store.archSet.TryGetValue(key, out var archetypeKey)) {
            return current.componentAdd.Cache((byte)structIndex, archetypeKey.archetype);
        }
        var config          = GetArchetypeConfig(store);
        var componentTypes  = current.componentTypes;
        componentTypes.bitSet.SetBit(structIndex);
        var archetype = Archetype.CreateWithComponentTypes(config, componentTypes, current.tags);
        AddArchetype(store, archetype);
        return current.componentAdd.Cache((byte)structIndex, archetype);
    }
    
    internal static Archetype GetArchetypeWithout(EntityStoreBase store, Archetype archetype, int structIndex)
    {
        var typeIndex = archetype.componentRemove.FindType(structIndex);
        if (typeIndex >= 0) {
            return archetype.store.archs[typeIndex];
        }
        var key = store.searchKey;
        key.SetWithout(archetype, structIndex);
        if (store.archSet.TryGetValue(key, out var archetypeKey)) {
            return archetype.componentRemove.Cache((byte)structIndex, archetypeKey.archetype);
        }
        var config          = GetArchetypeConfig(store);
        var componentTypes  = archetype.componentTypes;
        componentTypes.bitSet.ClearBit(structIndex);
        var result = Archetype.CreateWithComponentTypes(config, componentTypes, archetype.tags);
        AddArchetype(store, result);
        return archetype.componentRemove.Cache((byte)structIndex, result);
    }
    
    private static Archetype GetArchetypeWithTags(EntityStoreBase store, Archetype archetype, in Tags tags)
    {
        var key             = store.searchKey;
        key.componentTypes  = archetype.componentTypes;
        key.tags            = tags;
        key.CalculateHashCode();
        if (store.archSet.TryGetValue(key, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config  = GetArchetypeConfig(store);
        var result  = Archetype.CreateWithComponentTypes(config, archetype.componentTypes, tags);
        AddArchetype(store, result);
        return result;
    }
    
    internal Archetype GetArchetypeAdd(Archetype type, in ComponentTypes addComponents, in Tags addTags)
    {
        var key             = searchKey;
        key.tags            = type.tags;
        key.componentTypes  = type.componentTypes;    
        key.tags.bitSet.          Add(addTags.bitSet);
        key.componentTypes.bitSet.Add(addComponents.bitSet);
        if (key.componentTypes.bitSet.Equals(type.componentTypes.bitSet) &&
            key.tags.          bitSet.Equals(type.tags.          bitSet)) {
            return type;
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
    
    internal Archetype GetArchetypeRemove(Archetype type, in ComponentTypes removeComponents, in Tags removeTags)
    {
        var key             = searchKey;
        key.tags            = type.tags;
        key.componentTypes  = type.componentTypes;
        key.tags.bitSet          .Remove(removeTags.bitSet);
        key.componentTypes.bitSet.Remove(removeComponents.bitSet);
        if (key.componentTypes.bitSet.Equals(type.componentTypes.bitSet) &&
            key.tags.          bitSet.Equals(type.tags.          bitSet)) {
            return type;
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
