// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal readonly struct Archetypes
{
    internal readonly   Archetype[] array;          //  8
    internal readonly   int         length;         //  4
    internal readonly   int         last;           //  4
    internal readonly   int[]       chunkPositions; //  8

    public   override   string      ToString() => $"Archetype[{length}]";

    internal Archetypes(Archetype[] array, int length) {
        this.array  = array;
        this.length = length;
        last        = length - 1;
    }
    
    internal Archetypes(Archetype[] array, int length, int[] chunkPositions) {
        this.array          = array;
        this.length         = length;
        last                = length - 1;
        this.chunkPositions = chunkPositions;
    }
}

