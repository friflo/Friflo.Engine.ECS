// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class StructUtils
{
    internal const              int                                 ChunkSize = 512;
    internal const              int                                 MissingAttribute                = 0;
    
    private  static             int                                 _nextStructIndex                = 1;
//  private  static readonly    Dictionary<Type, string>            StructComponentKeys             = new Dictionary<Type, string>();
//  public   static             IReadOnlyDictionary<Type, string>   RegisteredStructComponentKeys   => StructComponentKeys;
    
    internal static int NewStructIndex(Type type, out string structKey) {
        foreach (var attr in type.CustomAttributes) {
            if (attr.AttributeType != typeof(StructComponentAttribute)) {
                continue;
            }
            var arg     = attr.ConstructorArguments;
            structKey   = (string) arg[0].Value;
        //  StructComponentKeys.Add(type, structKey);
            return _nextStructIndex++;
        }
        structKey = null;
        return MissingAttribute;
    }
}
