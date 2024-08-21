// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

internal struct Item
{
    [Browse(RootHidden)] private BaseSystem system;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(system.id);
        sb.Append(" - ");
        switch (system) {
            case QuerySystemBase querySystem:
                sb.Append(system.Name);
                sb.Append(" - entities: ");
                sb.Append(querySystem.EntityCount);
                break;
            case SystemGroup:
                sb.Append(system);
                break;
            default:
                sb.Append(system.Name);
                break;
        }
        return sb.ToString();
    }
        
    internal static Item[] GetAllSystems(SystemGroup systemGroup)
    {
        var systemBuffer = new ReadOnlyList<BaseSystem>(Array.Empty<BaseSystem>());
        systemGroup.GetSubSystems(ref systemBuffer);
        var result = new Item[systemBuffer.Count];
        for (int n = 0; n < systemBuffer.Count; n++) {
            result[n] = new Item { system = systemBuffer[n] };   
        }
        return result;
    }
}

