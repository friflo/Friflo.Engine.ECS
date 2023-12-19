// Copyright (c) Ullrich Praetz. All rights reserved.
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

    public   override       string          ToString() => GetString();
    
    internal ArchetypeKey() { }
    
    internal ArchetypeKey(Archetype archetype) {
        componentTypes  = archetype.componentTypes;
        tags            = archetype.tags;
        hash            = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
        this.archetype  = archetype;
    }
    
    internal void Clear() {
        componentTypes  = default;
        tags            = default;
        hash            = default;
    }
    
    internal void CalculateHashCode() {
        hash    = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetTagsWith(in Tags tags, int structIndex) {
        componentTypes  = default;
        componentTypes.SetBit(structIndex);
        this.tags       = tags;
        hash            = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetSignatureTags(in SignatureIndexes indexes, in Tags tags) {
        componentTypes  = new ComponentTypes(indexes);
        this.tags       = tags;
        hash            = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetWith(Archetype archetype, int structIndex) {
        componentTypes  = archetype.componentTypes;
        componentTypes.SetBit(structIndex);
        tags            = archetype.tags;
        hash            = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetWithout(Archetype archetype, int structIndex) {
        componentTypes  = archetype.componentTypes;
        componentTypes.ClearBit(structIndex);
        tags            = archetype.tags;
        hash            = componentTypes.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Key: [");
        var hasTypes = false;
        foreach (var componentType in componentTypes) {
            sb.Append(componentType.name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in tags) {
            sb.Append('#');
            sb.Append(tag.name);
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
        return left!.componentTypes.bitSet.value   == right!.componentTypes.bitSet.value &&
               left!.tags.bitSet.value             == right!.tags.bitSet.value;
    }

    public int GetHashCode(ArchetypeKey key) {
        return key.hash;
    }
}
