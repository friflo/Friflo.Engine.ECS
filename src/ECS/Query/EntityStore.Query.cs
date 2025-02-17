﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard rule: this file MUST NOT use type: Entity

// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public partial class EntityStoreBase
{
    
    #region relation
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given relation type.<br/>
    /// </summary>
    public ArchetypeQuery<T1> QueryRelation<T1> ()
        where T1 : struct, IRelation
    {
        var relationType = Static.EntitySchema.components[StructInfo<T1>.Index];
        return new ArchetypeQuery<T1>(this, Signature.GetRelation<T1>(), null, relationType);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1> QueryRelation<T1> (QueryFilter filter)
        where T1 : struct, IRelation
    {
        var relationType = Static.EntitySchema.components[StructInfo<T1>.Index];
        return new ArchetypeQuery<T1>(this, Signature.GetRelation<T1>(), filter, relationType);
    }
    #endregion
    
#region args - 0
    // -------------------------------------- archetype query --------------------------------------
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the entity store.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery Query ()
    {
        return new ArchetypeQuery(this, new ComponentTypes(), null);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery Query (QueryFilter filter)
    {
        return new ArchetypeQuery(this, new ComponentTypes(), filter);
    }
    #endregion

#region args - 1
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1> Query<T1> (Signature<T1> signature)
        where T1 : struct, IComponent
    {
        return new ArchetypeQuery<T1>(this, signature, null, null);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component type.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1> Query<T1> ()
        where T1 : struct, IComponent
    {
        return new ArchetypeQuery<T1>(this, Signature.Get<T1>(), null, null);
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1> Query<T1> (QueryFilter filter)
        where T1 : struct, IComponent
    {
        return new ArchetypeQuery<T1>(this, Signature.Get<T1>(), filter, null);
    }
    #endregion
    
#region args - 2
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> (Signature<T1, T2> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2>(this, signature, null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2>(this, Signature.Get<T1, T2>(), null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1, T2> Query<T1, T2> (QueryFilter filter)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        var result = new ArchetypeQuery<T1,T2>(this, Signature.Get<T1,T2>(), filter);
        return result;
    }
    #endregion
    
#region args - 3

    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> (Signature<T1, T2, T3> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3>(this, signature, null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3>(this, Signature.Get<T1, T2, T3>(), null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3> Query<T1, T2, T3> (QueryFilter filter)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        var result = new ArchetypeQuery<T1,T2,T3>(this, Signature.Get<T1,T2,T3>(), filter);
        return result;
    }
    #endregion
    
#region args - 4

    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> (Signature<T1, T2, T3, T4> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4>(this, signature, null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4>(this, Signature.Get<T1, T2, T3, T4>(), null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4> Query<T1, T2, T3, T4> (QueryFilter filter)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        var result = new ArchetypeQuery<T1,T2,T3,T4>(this, Signature.Get<T1,T2,T3,T4>(), filter);
        return result;
    }
    #endregion
    
#region args - 5
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> (Signature<T1, T2, T3, T4, T5> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4, T5>(this, signature, null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4, T5>(this, Signature.Get<T1, T2, T3, T4, T5>(), null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5> Query<T1, T2, T3, T4, T5> (QueryFilter filter)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
    {
        var result = new ArchetypeQuery<T1,T2,T3,T4,T5>(this, Signature.Get<T1,T2,T3,T4,T5>(), filter);
        return result;
    }
    #endregion
    
#region args - 6
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5, T6> Query<T1, T2, T3, T4, T5, T6> (Signature<T1, T2, T3, T4, T5, T6> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
        where T6: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4, T5, T6>(this, signature, null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5, T6> Query<T1, T2, T3, T4, T5, T6> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
        where T6: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4, T5, T6>(this, Signature.Get<T1, T2, T3, T4, T5, T6>(), null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5, T6> Query<T1, T2, T3, T4, T5, T6> (QueryFilter filter)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
    {
        var result = new ArchetypeQuery<T1,T2,T3,T4,T5,T6>(this, Signature.Get<T1,T2,T3,T4,T5,T6>(), filter);
        return result;
    }
#endregion

#region args - 7
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for given component <paramref name="signature"/>.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5, T6, T7> Query<T1, T2, T3, T4, T5, T6, T7> (Signature<T1, T2, T3, T4, T5, T6, T7> signature)
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
        where T6: struct, IComponent
        where T7: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4, T5, T6, T7>(this, signature, null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> for the given component types.<br/>
    /// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/documentation/query">Example.</a>
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5, T6, T7> Query<T1, T2, T3, T4, T5, T6, T7> ()
        where T1: struct, IComponent
        where T2: struct, IComponent
        where T3: struct, IComponent
        where T4: struct, IComponent
        where T5: struct, IComponent
        where T6: struct, IComponent
        where T7: struct, IComponent
    {
        var result = new ArchetypeQuery<T1, T2, T3, T4, T5, T6, T7>(this, Signature.Get<T1, T2, T3, T4, T5, T6, T7>(), null);
        return result;
    }
    
    /// <summary>
    /// Create a reusable <see cref="ArchetypeQuery"/> with given query <paramref name="filter"/>.<br/>
    /// The filter attached to the query can be modified subsequently.
    /// </summary>
    public ArchetypeQuery<T1, T2, T3, T4, T5, T6, T7> Query<T1, T2, T3, T4, T5, T6, T7> (QueryFilter filter)
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
        where T5 : struct, IComponent
        where T6 : struct, IComponent
        where T7 : struct, IComponent
    {
        var result = new ArchetypeQuery<T1,T2,T3,T4,T5,T6, T7>(this, Signature.Get<T1,T2,T3,T4,T5,T6, T7>(), filter);
        return result;
    }
#endregion
}
