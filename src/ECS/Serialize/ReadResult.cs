// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;

namespace Friflo.Engine.ECS.Serialize;

/// <summary>
/// Contains the aggregated result when reading entities with an <see cref="EntitySerializer"/>.
/// </summary>
public readonly struct ReadResult
{
    /// <summary> Number of entities returned by an <see cref="EntitySerializer"/> <c>Read</c> method. </summary>
    public readonly     int     entityCount;
    /// <summary>
    /// null - if reading entities was successful.<br/>
    /// Otherwise the error of an <see cref="EntitySerializer"/> <c>Read</c> method call.
    /// </summary>
    public readonly     string  error;
    
    internal ReadResult(int entityCount, string error) {
        this.entityCount    = entityCount;
        this.error          = error;
    }

    public override string ToString() => GetString();
    
    private string GetString() {
        var sb = new StringBuilder();
        sb.Append("entityCount: ");
        sb.Append(entityCount);
        if (error != null) {
            sb.Append(" error: ");
            sb.Append(error);
        }
        return sb.ToString();
    }
}