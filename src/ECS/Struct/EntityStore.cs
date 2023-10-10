// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static Friflo.Fliox.Engine.ECS.EntityStore.Static;

// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public sealed partial class EntityStore
{
    // -------------------------------------- get archetype --------------------------------------
#region get archetype
    private Archetype GetArchetype<T>(in Tags tags)
        where T : struct, IStructComponent
    {
        searchId.SetTagsWith(tags, StructHeap<T>.StructIndex);
        if (archetypeSet.TryGetValue(searchId, out var archetypeId)) {
            return archetypeId.type;
        }
        var config  = GetArchetypeConfig();
        var schema  = Static.ComponentSchema;
        var types   = new SignatureTypeSet(1,
            T1: schema.GetStructType(StructHeap<T>.StructIndex, typeof(T))
        );
        var archetype = Archetype.CreateWithSignatureTypes(config, types, tags);
        AddArchetype(archetype);
        return archetype;
    }
    
    internal ArchetypeConfig GetArchetypeConfig() {
        return new ArchetypeConfig (this, archetypesCount, DefaultCapacity);
    }
    
    private Archetype GetArchetypeWithSignature(Signature signature, in Tags tags)
    {
        searchId.SetSignature(signature, tags);
        if (archetypeSet.TryGetValue(searchId, out var archetypeId)) {
            return archetypeId.type;
        }
        var config      = GetArchetypeConfig();
        var archetype   = Archetype.CreateWithSignatureTypes(config, signature.types, tags);
        AddArchetype(archetype);
        return archetype;
    }
    
    public bool TryGetArchetypeId(in ArchetypeId id, out ArchetypeId actualValue) {
        return archetypeSet.TryGetValue(id, out actualValue);
    }

    public Archetype GetArchetype<T>(Signature<T> signature, in Tags tags = default)
        where T : struct, IStructComponent
    {
        return GetArchetypeWithSignature(signature, tags);
    }
    
    public Archetype GetArchetype<T1, T2>(Signature<T1, T2> signature, in Tags tags = default)
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
    {
        return GetArchetypeWithSignature(signature, tags);
    }
    
    public Archetype GetArchetype<T1, T2, T3>(Signature<T1, T2, T3> signature, in Tags tags = default)
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
    {
        return GetArchetypeWithSignature(signature, tags);
    }
    
    public Archetype GetArchetype<T1, T2, T3, T4>(Signature<T1, T2, T3, T4> signature, in Tags tags = default)
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
        where T4 : struct, IStructComponent
    {
        return GetArchetypeWithSignature(signature, tags);
    }
    
    public Archetype GetArchetype<T1, T2, T3, T4, T5>(Signature<T1, T2, T3, T4, T5> signature, in Tags tags = default)
        where T1 : struct, IStructComponent
        where T2 : struct, IStructComponent
        where T3 : struct, IStructComponent
        where T4 : struct, IStructComponent
        where T5 : struct, IStructComponent
    {
        return GetArchetypeWithSignature(signature, tags);
    }
    #endregion
    
    // -------------------------------------- archetype query --------------------------------------
#region archetype query
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the <see cref="EntityStore"/>
    /// </summary>
    public ArchetypeQuery<T> Query<T> (Signature<T> signature)
        where T : struct, IStructComponent
    {
        return new ArchetypeQuery<T>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the <see cref="EntityStore"/>
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> (Signature<T1, T2> signature, Tags tags = default)
        where T1: struct, IStructComponent
        where T2: struct, IStructComponent
    {
        return new ArchetypeQuery<T1, T2>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the <see cref="EntityStore"/>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> (Signature<T1, T2, T3> signature)
        where T1: struct, IStructComponent
        where T2: struct, IStructComponent
        where T3: struct, IStructComponent
    {
        return new ArchetypeQuery<T1, T2, T3>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the <see cref="EntityStore"/>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> (Signature<T1, T2, T3, T4> signature)
        where T1: struct, IStructComponent
        where T2: struct, IStructComponent
        where T3: struct, IStructComponent
        where T4: struct, IStructComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the <see cref="EntityStore"/>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> (Signature<T1, T2, T3, T4, T5> signature)
        where T1: struct, IStructComponent
        where T2: struct, IStructComponent
        where T3: struct, IStructComponent
        where T4: struct, IStructComponent
        where T5: struct, IStructComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4, T5>(this, signature);
    }
    
    #endregion
}
