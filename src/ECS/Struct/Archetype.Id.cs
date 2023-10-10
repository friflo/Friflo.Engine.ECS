// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <remarks>
/// As the <see cref="ArchetypeId"/> is a large struct (72 bytes) it performs much better as a class than a struct
/// in a <see cref="HashSet{T}"/><br/>
/// The <see cref="IEqualityComparer{T}"/> requires two copies of an <see cref="ArchetypeId"/>
/// </remarks>
public sealed class ArchetypeId
{
    internal            ArchetypeStructs    structs;    // 32
    internal            Tags                tags;       // 32
    internal            int                 hash;       //  4
    public   readonly   Archetype           type;       //  8

    public   override   string              ToString() => GetString();

    internal ArchetypeId() { }
    
    internal ArchetypeId(Archetype archetype) {
        structs = archetype.structs;
        tags    = archetype.tags;
        hash    = structs.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
        type    = archetype;
    }
    
    internal void Clear() {
        structs = default;
        tags    = default;
        hash    = default;
    }
    
    internal void CalculateHashCode() {
        hash    = structs.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetTagsWith(in Tags tags, int structIndex) {
        structs         = default;
        structs.bitSet.SetBit(structIndex);
        this.tags       = tags;
        hash            = structs.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetMaskTags(in ArchetypeStructs structs, in Tags tags) {
        this.structs    = structs;
        this.tags       = tags;
        hash            = structs.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetWith(Archetype archetype, int structIndex) {
        structs         = archetype.structs;
        structs.bitSet.SetBit(structIndex);
        tags            = archetype.tags;
        hash            = structs.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetWithout(Archetype archetype, int structIndex) {
        structs         = archetype.structs;
        structs.bitSet.ClearBit(structIndex);
        tags            = archetype.tags;
        hash            = structs.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Id: [");
        var hasTypes = false;
        foreach (var structType in structs) {
            sb.Append(structType.type.Name);
            sb.Append(", ");
            hasTypes = true;
        }
        foreach (var tag in tags) {
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

internal sealed class ArchetypeIdEqualityComparer : IEqualityComparer<ArchetypeId>
{
    internal static readonly ArchetypeIdEqualityComparer Instance = new ();

    public bool Equals(ArchetypeId x, ArchetypeId y) {
        return x!.structs.bitSet.value == y!.structs.bitSet.value &&
               x!.tags.bitSet.value == y!.tags.bitSet.value;
    }

    public int GetHashCode(ArchetypeId archetype) {
        return archetype.hash;
    }
}
