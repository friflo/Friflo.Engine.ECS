// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal sealed class FindKey
{
    // --- internal fields
    internal                ComponentTypes  componentTypes; // 32   - components of an Archetype
    internal                Tags            tags;           // 32   - tags of an Archetype
    internal                int             hash;           //  4   - hash code from components & tags
    internal readonly       ArchetypeQuery  query;          //  8   - the result of a key lookup

    internal FindKey() { }
    internal FindKey(in ComponentTypes componentTypes, in Tags tags, int hash, ArchetypeQuery query) {
        this.componentTypes = componentTypes;
        this.tags           = tags;
        this.hash           = hash;
        this.query          = query;
    }
}

internal sealed class FindKeyEqualityComparer : IEqualityComparer<FindKey>
{
    internal static readonly FindKeyEqualityComparer Instance = new ();

    public bool Equals(FindKey left, FindKey right) {
        return left!.componentTypes.bitSet.value   == right!.componentTypes.bitSet.value &&
               left!.tags.bitSet.value             == right!.tags.bitSet.value;
    }

    public int GetHashCode(FindKey key) {
        return key.hash;
    }
}


public partial class EntityStoreBase
{
    private readonly    HashSet<FindKey>    findQueryCache  = new (FindKeyEqualityComparer.Instance);   // todo move to InternBase
    private readonly    FindKey             findQueryKey    = new ();                                   // todo move to InternBase
    
#region public find methods
    [Obsolete("experimental")]
    public Entity FindEntity        (in Tags withAllTags = default)
    {
        var findQuery           = GetFindQuery(default, withAllTags);
        return GetSingleEntity(findQuery);
    }
    
    [Obsolete("experimental")]
    public Entity FindEntity<T1>    (in Tags withAllTags = default)
        where T1: struct, IComponent
    {
        var signatureIndexes    = Signature.Get<T1>().signatureIndexes;
        var findQuery           = GetFindQuery(signatureIndexes, withAllTags);
        return GetSingleEntity(findQuery);
    }
    
    [Obsolete("experimental")]
    public Entity FindEntity<T1, T2> (in Tags withAllTags = default)
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        var signatureIndexes    = Signature.Get<T1, T2>().signatureIndexes;
        var findQuery           = GetFindQuery(signatureIndexes, withAllTags);
        return GetSingleEntity(findQuery);
    }
    
    [Obsolete("experimental")]
    public Entity FindEntity<T1, T2, T3> (in Tags withAllTags = default)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        var signatureIndexes    = Signature.Get<T1, T2, T3>().signatureIndexes;
        var findQuery           = GetFindQuery(signatureIndexes, withAllTags);
        return GetSingleEntity(findQuery);
    }
    
    [Obsolete("experimental")]
    public Entity FindEntity<T1, T2, T3, T4> (in Tags withAllTags = default)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        var signatureIndexes    = Signature.Get<T1, T2, T3, T4>().signatureIndexes;
        var findQuery           = GetFindQuery(signatureIndexes, withAllTags);
        return GetSingleEntity(findQuery);
    }
    
    [Obsolete("experimental")]
    public Entity FindEntity<T1, T2, T3, T4, T5> (in Tags withAllTags = default)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
    {
        var signatureIndexes    = Signature.Get<T1, T2, T3, T4, T5>().signatureIndexes;
        var findQuery           = GetFindQuery(signatureIndexes, withAllTags);
        return GetSingleEntity(findQuery);
    }
    #endregion


#region private find methods
    private Entity GetSingleEntity (ArchetypeQuery findQuery)
    {
        // --- check if query contains exact one entity 
        var queryEntityCount = findQuery.EntityCount;
        switch (queryEntityCount) {
            case 0:
                throw new InvalidOperationException("found no matching entity");
            case > 1:
                throw new InvalidOperationException($"found multiple matching entities. found: {queryEntityCount}");
        }
        
        // --- get the one entity
        var archetypes      = findQuery.GetArchetypes();
        Archetype archetype = null;
        for (int archIndex = 0; archIndex < archetypes.length; archIndex++) {
            archetype = archetypes.array[archIndex];
            if (archetype.entityCount == 0) {
                continue;
            }
            break;
        }
        var entityId = archetype!.entityIds[0];
        return new Entity((EntityStore)this, entityId);
    }
    
    private ArchetypeQuery GetFindQuery(in SignatureIndexes signatureIndexes, in Tags withAllTags)
    {
        var componentTypes          = new ComponentTypes(signatureIndexes);
        var hash                    = componentTypes.bitSet.HashCode() ^ withAllTags.bitSet.HashCode();
        findQueryKey.componentTypes = componentTypes;
        findQueryKey.tags           = withAllTags;
        findQueryKey.hash           = hash;
        if (findQueryCache.TryGetValue(findQueryKey, out var value)) {
            return value.query;
        }
        var query = new ArchetypeQuery(this, signatureIndexes);
        query.AllTags(withAllTags);
        var key = new FindKey(componentTypes, withAllTags, hash, query);
        findQueryCache.Add(key);
        return query;
    }
    #endregion
}
