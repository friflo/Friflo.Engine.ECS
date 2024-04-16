// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

#if NETCOREAPP3_0_OR_GREATER
    using System.Numerics;
#endif


// ReSharper disable ConvertToAutoProperty
namespace Friflo.Engine.ECS.Utils;


public partial struct BitSet
{
    // Compare
    // ------------------------------ IsDefault() - Equals() ------------------------------
#if NETCOREAPP3_0_OR_GREATER
    internal bool IsDefault() {
        return value.Equals(default);
    }

    internal bool Equals(in BitSet other) {
        return value.Equals(other.value);
    }

    public readonly int GetBitCount() {
        return
            BitOperations.PopCount((ulong)l0) +
            BitOperations.PopCount((ulong)l1) +
            BitOperations.PopCount((ulong)l2) + 
            BitOperations.PopCount((ulong)l3);
    }
#endif
    
    // Compare
    // ------------------------------------ bit operations ------------------------------------
#if NET7_0_OR_GREATER
    public readonly bool HasAll(in BitSet bitSet)
    {
        return (value & bitSet.value) == bitSet.value;
    }
    
    public readonly bool HasAny(in BitSet bitSet)
    {
        return !(value & bitSet.value).Equals(default);
    }

    internal void Add (in BitSet right) {
        value |= right.value;
    }
    
    internal void Remove (in BitSet right) {
        value &= ~right.value;
    }
    
    internal static BitSet Add (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value | right.value
        };
    }
    
    internal static BitSet Remove (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value & ~right.value
         };
    }
    
    internal static BitSet Added (in BitSet left, in BitSet right) {
        return new BitSet {
            value = ~left.value & right.value
        };
    }
    
    internal static BitSet Removed (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value & ~right.value
        };
    }
    
    internal static BitSet Changed (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value ^ right.value
        };
    }
    
    internal static BitSet Intersect (in BitSet left, in BitSet right) {
        return new BitSet {
            value = left.value & right.value
        };
    }
#endif

}
