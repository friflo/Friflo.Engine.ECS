// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public abstract class Tags
{
    internal readonly long tagHash;
    
    // --- static
    private static readonly Dictionary<long, Tags>    TagMap = new Dictionary<long, Tags>();
    
    internal Tags(long tagHash) {
        this.tagHash = tagHash;
    }
        
    public static Tags<T> Get<T>()
        where T : struct, IEntityTag
    {
        var hash = typeof(T).Handle();
        if (TagMap.TryGetValue(hash, out var result)) {
            return (Tags<T>)result;
        }
        var tags = new Tags<T>(hash);
        TagMap.Add(hash, tags);
        return tags;
    }
    
    public static Tags<T1, T2> Get<T1, T2>()
        where T1 : struct, IEntityTag
        where T2 : struct, IEntityTag
    {
        var hash = typeof(T1).Handle() ^
                   typeof(T2).Handle();
        if (TagMap.TryGetValue(hash, out var result)) {
            return (Tags<T1, T2>)result;
        }
        var tags = new Tags<T1, T2>(hash);
        TagMap.Add(hash, tags);
        return tags;
    }
}

public sealed class Tags<T> : Tags
    where T : struct, IEntityTag
{
    internal Tags(long tagHash) : base(tagHash) { }
}

public sealed class Tags<T1, T2> : Tags
    where T1 : struct, IEntityTag
    where T2 : struct, IEntityTag
{
    internal Tags(long tagHash) : base(tagHash) { }
}


