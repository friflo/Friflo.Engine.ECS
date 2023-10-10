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
    internal            ArchetypeMask   mask;   // 32
    internal            Tags            tags;   // 32
    internal            int             hash;   //  4
    public   readonly   Archetype       type;   //  8

    public   override   string          ToString() => GetString();

    internal ArchetypeId() { }
    
    internal ArchetypeId(Archetype archetype) {
        mask    = archetype.mask;
        tags    = archetype.tags;
        hash    = mask.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
        type    = archetype;
    }
    
    internal void Clear() {
        mask = default;
        tags = default;
        hash = default;
    }
    
    internal void CalculateHashCode() {
        hash        = mask.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetTagsWith(in Tags tags, int structIndex) {
        mask        = default;
        mask.bitSet.SetBit(structIndex);
        this.tags   = tags;
        hash        = mask.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetMaskTags(in ArchetypeMask mask, in Tags tags) {
        this.mask   = mask;
        this.tags   = tags;
        hash        = mask.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetWith(Archetype archetype, int structIndex) {
        mask        = archetype.mask;
        mask.bitSet.SetBit(structIndex);
        tags        = archetype.tags;
        hash        = mask.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    internal void SetWithout(Archetype archetype, int structIndex) {
        mask        = archetype.mask;
        mask.bitSet.ClearBit(structIndex);
        tags        = archetype.tags;
        hash        = mask.bitSet.GetHashCode() ^ tags.bitSet.GetHashCode();
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();
        sb.Append("Id: [");
        var hasTypes = false;
        foreach (var structType in mask) {
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
        return x!.mask.bitSet.value == y!.mask.bitSet.value &&
               x!.tags.bitSet.value == y!.tags.bitSet.value;
    }

    public int GetHashCode(ArchetypeId archetype) {
        return archetype.hash;
    }
}
