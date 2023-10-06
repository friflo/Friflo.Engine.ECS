// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

[StructLayout(LayoutKind.Explicit)]
internal struct DecomposedGuid
{
    [FieldOffset(00)] public Guid Value;
    [FieldOffset(00)] public long Hi;
    [FieldOffset(08)] public long Lo;
    public DecomposedGuid(Guid value) : this() => Value = value;
}

internal readonly struct ArchetypeInfo
{
    public   readonly   Archetype   type;
    internal readonly   long        hash;
    
    public ArchetypeInfo(long hash, Archetype type) {
        this.type   = type;
        this.hash   = hash;
    }
}

// ReSharper disable InconsistentNaming
internal struct StructIndexes
{
    internal int T1;
    internal int T2;
    internal int T3;
    internal int T4;
    internal int T5;
}
