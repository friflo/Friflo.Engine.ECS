// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    // -------------------------------------- archetype query --------------------------------------
#region archetype query
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
    /// </summary>
    public ArchetypeQuery Query ()
    {
        return new ArchetypeQuery(this, new ComponentTypes());
    }

    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1> Query<T1> (in Signature<T1> signature)
        where T1 : struct, IComponent
    {
        return new ArchetypeQuery<T1>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component type.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1> Query<T1> ()
        where T1 : struct, IComponent
    {
        return new ArchetypeQuery<T1>(this, Signature.Get<T1>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> (in Signature<T1, T2> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2>(this, Signature.Get<T1, T2>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> (in Signature<T1, T2, T3> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3>(this, Signature.Get<T1, T2, T3>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> (in Signature<T1, T2, T3, T4> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4>(this, signature);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        return new ArchetypeQuery<T1, T2, T3, T4>(this, Signature.Get<T1, T2, T3, T4>());
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
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
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://github.com/friflo/Friflo.Json.Fliox/blob/main/Engine/README.md#query-entities">Example.</a>
    /// </summary>
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
