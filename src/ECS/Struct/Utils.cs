using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Fliox.Engine.ECS;

public static class StructUtils
{
    internal const              int                                 ChunkSize = 512;
    internal const              int                                 MissingAttribute    = 0;
    
    private  static             int                                 _nextComponentIndex = 1;
    private  static readonly    Dictionary<Type, string>            Types           = new Dictionary<Type, string>();
    public   static             IReadOnlyDictionary<Type, string>   RegisteredTypes => Types;
    
    internal static int NewComponentIndex(Type type, out string key) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType == typeof(StructComponentAttribute)) {
                var arg = attr.ConstructorArguments;
                key     = (string) arg[0].Value;
                Types.Add(type, key);
                return _nextComponentIndex++;
            }
        }
        key = null;
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