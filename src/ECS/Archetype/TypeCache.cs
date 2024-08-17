// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal struct TypeCache
{
    private byte    type0;  //  1
    private byte    type1;  //  1
    private byte    type2;  //  1
    private byte    type3;  //  1
    
    private int     index0; //  4
    private int     index1; //  4
    private int     index2; //  4
    private int     index3; //  4
    
    private int     next;   //  4
    
    internal int FindType(int type)
    {
        if (type == type0)  return index0;
        if (type == type1)  return index1;
        if (type == type2)  return index2;
        if (type == type3)  return index3;
        return -1;
    }
    
    internal Archetype Cache(byte type, Archetype archetype)
    {
        var index   = archetype.archIndex;
        var at      = next++ % 4;
        switch (at) {
            case 0: type0 = type; index0 = index; break;
            case 1: type1 = type; index1 = index; break;
            case 2: type2 = type; index2 = index; break;
            case 3: type3 = type; index3 = index; break;
        }
        return archetype;
    }
}