// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <remarks>
/// As the <see cref="ArchetypeKey"/> requires ~72 bytes it performs much better as a class than a struct
/// in a <see cref="HashSet{T}"/><br/>
/// The <see cref="IEqualityComparer{T}"/> requires two copies of an <see cref="ArchetypeKey"/>
/// </remarks>
internal sealed class ArchetypeKey
{
    // --- internal fields
    internal                ArchetypeStructs    structs;    // 32   - components of an Archetype
    internal                Tags                tags;       // 32   - tags of an Archetype
    internal                int                 hash;       //  4   - hash code from structs & tags
    internal readonly       Archetype           archetype;  //  8   - the result of a key lookup

    public   override       string              ToString() => GetString();
    
    internal ArchetypeKey() { }
    
    internal ArchetypeKey(Archetype archetype) {
        structs         = archetype.structs;
        tags            = archetype.tags;
        hash            = structs.bitSet.HashCode() ^ tags.bitSet.HashCode();
        this.archetype  = archetype;
    }
    
    internal void Clear() {
        structs = default;
        tags    = default;
        hash    = default;
    }
    
    internal void CalculateHashCode() {
        hash    = structs.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetTagsWith(in Tags tags, int structIndex) {
        structs         = default;
        structs.SetBit(structIndex);
        this.tags       = tags;
        hash            = structs.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetSignatureTags(in SignatureIndexes indexes, in Tags tags) {
        structs         = new ArchetypeStructs(indexes);
        this.tags       = tags;
        hash            = structs.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetWith(Archetype archetype, int structIndex) {
        structs         = archetype.structs;
        structs.SetBit(structIndex);
        tags            = archetype.tags;
        hash            = structs.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    internal void SetWithout(Archetype archetype, int structIndex) {
        structs         = archetype.structs;
        structs.ClearBit(structIndex);
        tags            = archetype.tags;
        hash            = structs.bitSet.HashCode() ^ tags.bitSet.HashCode();
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Key: [");
        var hasTypes = false;
        foreach (var structType in structs) {
            sb.Append(structType.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in tags) {
            sb.Append('#');
            sb.Append(tag.type.Name);
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
        return left!.structs.bitSet.value   == right!.structs.bitSet.value &&
               left!.tags.bitSet.value      == right!.tags.bitSet.value;
    }

    public int GetHashCode(ArchetypeKey key) {
        return key.hash;
    }
}
