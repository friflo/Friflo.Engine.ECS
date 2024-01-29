// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[ExcludeFromCodeCoverage]
internal struct EntityEvents
{
#region properties
    internal        ReadOnlySpan<int>   EntityIds   => new (entityIds, 0, entityIdCount);

    public override string              ToString()  => GetString();

    #endregion
    
#region fields
    internal            int[]           entityIds;      //  8   - never null
    internal            int             entityIdCount;  //  4
    internal            HashSet<int>    entitySet;      //  8   - can be null. Created / updated on demand.
    internal            int             entitySetPos;   //  4
    private  readonly   SchemaType      type;           //  8
    #endregion
    
    internal EntityEvents(SchemaType type) {
        this.type = type;
    }
    
    internal bool ContainsId(int entityId)
    {
        var idCount = entityIdCount;
        var set     = entitySet ??= new HashSet<int>(idCount);
        if (entitySetPos < idCount) {
            UpdateHashSet();
        }
        return set.Contains(entityId);
    }
    
    internal void UpdateHashSet()
    {
        var set = entitySet;
        var ids = new ReadOnlySpan<int>(entityIds, entitySetPos, entityIdCount - entitySetPos);
        foreach (var id in ids) {
            set.Add(id);
        }
        entitySetPos = entityIdCount;
    }
    
    private string GetString() {
        if (type == null) {
            return "";
        }
        string marker = type.Kind == SchemaTypeKind.Component ? "" : "#";
        return $"[{marker}{type.Name}] events: {entityIdCount}";
    }
}

