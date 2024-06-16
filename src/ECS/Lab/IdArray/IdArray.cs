// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal struct IdArray
{
    public              int     Count => count;
    
    internal            int     start;
    internal readonly   int     count;

    public   override   string  ToString() =>$"count: {count}";

    internal IdArray(int start, int count) {
        this.start = start;
        this.count = count;
    }
}