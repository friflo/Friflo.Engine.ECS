// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public static class StructUtils
{
    internal const              int                                 ChunkSize = 512;
    internal const              int                                 MissingAttribute    = 0;
    
    private  static             int                                 _nextStructIndex    = 1;
    private  static readonly    Dictionary<Type, string>            Types               = new Dictionary<Type, string>();
    public   static             IReadOnlyDictionary<Type, string>   RegisteredTypes => Types;
    
    internal static int NewStructIndex(Type type, out string structKey) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType == typeof(StructComponentAttribute)) {
                var arg     = attr.ConstructorArguments;
                structKey   = (string) arg[0].Value;
                Types.Add(type, structKey);
                return _nextStructIndex++;
            }
        }
        structKey = null;
        return MissingAttribute;
    }
}

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