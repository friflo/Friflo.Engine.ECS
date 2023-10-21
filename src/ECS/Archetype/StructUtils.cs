// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public static class StructInfo
{
    public   const  int     ChunkSize           = 512; // check 64
    internal const  int     MissingAttribute    = 0;
}

internal static class StructUtils
{
    private static  int     _nextStructIndex    = 1;
    
    internal static int NewStructIndex(Type type, out string structKey) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(StructComponentAttribute)) {
                continue;
            }
            var arg     = attr.ConstructorArguments;
            structKey   = (string) arg[0].Value;
            return _nextStructIndex++;
        }
        structKey = null;
        return StructInfo.MissingAttribute;
    }
}
