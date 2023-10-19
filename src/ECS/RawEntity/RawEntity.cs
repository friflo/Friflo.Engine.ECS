// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

/// <remarks>
/// <b>Hard rule</b><br/>
/// All fields must be blittable types. As the intention is to store millions (or billions) of <see cref="RawEntity"/>'s
/// in <see cref="RawEntityStore"/>.<see cref="RawEntityStore.entities"/>.<br/>
/// This enables that the GC will not iterate <see cref="RawEntityStore.entities"/> when performing a GC.Collect().
/// </remarks>
internal struct RawEntity
{
    internal        int     archIndex;  // 4    could be short. if changing check perf
    internal        int     compIndex;  // 4

    public override readonly string  ToString() => $"archIndex: {archIndex}  compIndex: {compIndex}";
}
