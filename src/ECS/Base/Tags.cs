// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public class Tags
{
    internal readonly long tagHash;
    
    // --- static
    private static readonly Dictionary<long, Tags>    TagMap = new Dictionary<long, Tags>();
    
    private Tags(long tagHash) {
        this.tagHash = tagHash;
    }
        
    public static Tags Get<T>()
        where T : struct, IEntityTag
    {
        var hash = typeof(T).Handle();
        if (TagMap.TryGetValue(hash, out var result)) {
            return result;
        }
        var tags = new Tags(hash);
        TagMap.Add(hash, tags);
        return tags;
    }
    
    public static Tags Get<T1, T2>()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        if (TagMap.TryGetValue(hash, out var result)) {
            return result;
        }
        var tags = new Tags(hash);
        TagMap.Add(hash, tags);
        return tags;
    }
}


