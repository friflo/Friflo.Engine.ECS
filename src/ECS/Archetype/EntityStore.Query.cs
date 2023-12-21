// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    // -------------------------------------- get archetype --------------------------------------
#region get archetype
    private static Archetype GetArchetype(EntityStoreBase store, in Tags tags, int structIndex)
    {
        var searchKey = store.searchKey;
        searchKey.SetTagsWith(tags, structIndex);
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config  = GetArchetypeConfig(store);
        var schema  = Static.EntitySchema;
        var types   = new SignatureIndexes(1,
            T1: schema.CheckStructIndex(null, structIndex)
        );
        var archetype = Archetype.CreateWithSignatureTypes(config, types, tags);
        AddArchetype(store, archetype);
        return archetype;
    }
    
    internal static ArchetypeConfig GetArchetypeConfig(EntityStoreBase store) {
        return new ArchetypeConfig (store, store.archsCount);
    }
    
    private static Archetype GetArchetypeWithSignature(EntityStoreBase store, in SignatureIndexes indexes, in Tags tags)
    {
        var searchKey = store.searchKey;
        searchKey.SetSignatureTags(indexes, tags);
        if (store.archSet.TryGetValue(searchKey, out var archetypeKey)) {
            return archetypeKey.archetype;
        }
        var config      = GetArchetypeConfig(store);
        var archetype   = Archetype.CreateWithSignatureTypes(config, indexes, tags);
        AddArchetype(store, archetype);
        return archetype;
    }
    
    public Archetype FindArchetype(in ComponentTypes componentTypes, in Tags tags) {
        searchKey.componentTypes    = componentTypes;
        searchKey.tags              = tags;
        searchKey.CalculateHashCode();
        archSet.TryGetValue(searchKey, out var actualValue);
        return actualValue?.archetype;
    }
    
    public Archetype GetArchetype(in Tags tags)
    {
        return GetArchetypeWithSignature(this, default, tags);
    }

    public Archetype GetArchetype<T>(in Signature<T> signature, in Tags tags = default)
        where T : struct, IComponent
    {
        return GetArchetypeWithSignature(this, signature.signatureIndexes, tags);
    }
    
    public Archetype GetArchetype<T1, T2>(in Signature<T1, T2> signature, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        return GetArchetypeWithSignature(this, signature.signatureIndexes, tags);
    }
    
    public Archetype GetArchetype<T1, T2, T3>(in Signature<T1, T2, T3> signature, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        return GetArchetypeWithSignature(this, signature.signatureIndexes, tags);
    }
    
    public Archetype GetArchetype<T1, T2, T3, T4>(in Signature<T1, T2, T3, T4> signature, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        return GetArchetypeWithSignature(this, signature.signatureIndexes, tags);
    }
    
    public Archetype GetArchetype<T1, T2, T3, T4, T5>(in Signature<T1, T2, T3, T4, T5> signature, in Tags tags = default)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        return GetArchetypeWithSignature(this, signature.signatureIndexes, tags);
    }
    #endregion
    
    // -------------------------------------- archetype query --------------------------------------
#region archetype query
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store
    /// </summary>
    public ArchetypeQuery Query ()
    {
        return new ArchetypeQuery(this, default);
    }

    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store
    /// </summary>
    public ArchetypeQuery<T> Query<T> (in Signature<T> signature)
        where T : struct, IComponent
    {
        return new ArchetypeQuery<T>(this, signature);
    }
    
    public ArchetypeQuery<T> Query<T> ()
        where T : struct, IComponent
    {
        return new ArchetypeQuery<T>(this, Signature.Get<T>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> (in Signature<T1, T2> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2>(this, signature);
    }
    
    public ArchetypeQuery<T1, T2> Query<T1, T2> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2>(this, Signature.Get<T1, T2>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> (in Signature<T1, T2, T3> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3>(this, signature);
    }
    
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3>(this, Signature.Get<T1, T2, T3>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> (in Signature<T1, T2, T3, T4> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4>(this, signature);
    }
    
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4>(this, Signature.Get<T1, T2, T3, T4>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> (in Signature<T1, T2, T3, T4, T5> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4, T5>(this, signature);
    }
    
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4, T5>(this, Signature.Get<T1, T2, T3, T4, T5>());
    }
    
    #endregion
}
