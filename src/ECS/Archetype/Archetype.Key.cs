// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// As the <see cref="ArchetypeKey"/> requires ~72 bytes it performs much better as a class than a struct
/// in a <see cref="HashSet{T}"/><br/>
/// The <see cref="IEqualityComparer{T}"/> requires two copies of an <see cref="ArchetypeKey"/>
/// </remarks>
internal sealed class ArchetypeKey
{
    // --- internal fields
    internal                ComponentTypes  componentTypes; // 32   - components of an Archetype
    internal                Tags            tags;           // 32   - tags of an Archetype
    internal                int             hash;           //  4   - hash code from components & tags
    internal readonly       Archetype       archetype;      //  8   - the result of a key lookup

    public   override       string          ToString() => this.GetString();
    
    internal ArchetypeKey() { }
    
    internal ArchetypeKey(Archetype archetype) {
        componentTypes  = archetype.componentTypes;
        tags            = archetype.tags;
        hash            = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
        this.archetype  = archetype;
    }
}

internal static class ArchetypeKeyExtensions {
    
    internal static void Clear(this ArchetypeKey key) {
        key.componentTypes  = default;
        key.tags            = default;
        key.hash            = default;
    }
    
    internal static void CalculateHashCode(this ArchetypeKey key) {
        key.hash            = key.componentTypes.bitSet.HashCode() ^ key.tags.bitSet.HashCode();
    }
    
    internal static void SetWith(this ArchetypeKey key, Archetype archetype, int structIndex) {
        key.componentTypes  = archetype.componentTypes;
        key.componentTypes.bitSet.SetBit(structIndex);
        key.tags            = archetype.tags;
        key.hash            = key.componentTypes.bitSet.HashCode() ^ key.tags.bitSet.HashCode();
    }
    
    internal static void SetWithout(this ArchetypeKey key, Archetype archetype, int structIndex) {
        key.componentTypes  = archetype.componentTypes;
        key.componentTypes.bitSet.ClearBit(structIndex);
        key.tags            = archetype.tags;
        key.hash            = key.componentTypes.bitSet.HashCode() ^ key.tags.bitSet.HashCode();
    }
    
    internal static string GetString(this ArchetypeKey key)
    {
        var sb = new StringBuilder();
        sb.Append("Key: [");
        var hasTypes = false;
        foreach (var componentType in key.componentTypes) {
            sb.Append(componentType.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in key.tags) {
            sb.Append('#');
            sb.Append(tag.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        if (hasTypes) {
            sb.Length -= 2;
        }
        sb.Append(']');
        return sb.ToString();
    }
} 

internal sealed class ArchetypeKeyEqualityComparer : IEqualityComparer<ArchetypeKey>
{
    internal static readonly ArchetypeKeyEqualityComparer Instance = new ();

    public bool Equals(ArchetypeKey left, ArchetypeKey right) {
        return left!.componentTypes.bitSet.Equals(right!.componentTypes.bitSet) &&
               left!.tags.          bitSet.Equals(right!.tags.          bitSet);
    }

    public int GetHashCode(ArchetypeKey key) {
        return key.hash;
    }
}
