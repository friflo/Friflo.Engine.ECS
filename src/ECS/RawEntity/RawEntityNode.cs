// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <remarks>
/// <b>Hard rule</b><br/>
/// All fields must be blittable types. As the intention is to store millions (or billions) of <see cref="RawEntityNode"/>'s
/// in <see cref="RawEntityStore"/>.<see cref="RawEntityStore.entities"/>.<br/>
/// This enables that the GC will not iterate <see cref="RawEntityStore.entities"/> when performing a GC.Collect().
/// </remarks>
internal struct RawEntityNode
{
    internal        int     archIndex;  // 4    could be short. if changing check perf
    internal        int     compIndex;  // 4

    public readonly override string  ToString() => $"archIndex: {archIndex}  compIndex: {compIndex}";
}
