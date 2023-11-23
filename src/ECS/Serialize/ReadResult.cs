// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;

namespace Friflo.Fliox.Engine.ECS.Serialize;

public readonly struct ReadResult
{
    public readonly     int     entityCount;
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