// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS.Index;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Contains component and tags filters added to an <see cref="ArchetypeQuery"/>.<br/>
/// Multiple <see cref="ArchetypeQuery"/>'s can share a single <see cref="QueryFilter"/> instance.
/// </summary>
public class QueryFilter
{
    /// <summary>
    /// Contains component and tag filter conditions added to an <see cref="ArchetypeQuery"/>. 
    /// </summary>
    public readonly struct FilterCondition
    {
        /** Entity must have all tags. */               public    Tags            AllTags               => filter.allTags;
        /** Entity must have any tag. */                public    Tags            AnyTags               => filter.anyTags;
        /** Entity must not have all tags. */           public    Tags            WithoutAllTags        => filter.withoutAllTags;
        /** Entity must not have any tag. */            public    Tags            WithoutAnyTags        => filter.withoutAnyTags;
                        
        /** Entity must have all component types. */    public    ComponentTypes  AllComponents         => filter.allComponents;
        /** Entity must have any component types. */    public    ComponentTypes  AnyComponents         => filter.anyComponents;
        /** Entity must not have all component types. */public    ComponentTypes  WithoutAllComponents  => filter.withoutAllComponents;
        /** Entity must not have any component types. */public    ComponentTypes  WithoutAnyComponents  => filter.withoutAnyComponents;
    
        [Browse(Never)] private readonly QueryFilter filter;
    
        internal FilterCondition(QueryFilter filter) {
            this.filter = filter;
        }
    }
    
#region public fields
    /// <summary> Return all filter conditions of <see cref="QueryFilter"/>. </summary>
                public readonly FilterCondition Condition;                  //   8
    #endregion
    
#region private fields
    [Browse(Never)] private     Tags            allTags;                    //  32  entity must have all tags
    [Browse(Never)] private     Tags            anyTags;                    //  32  entity must have any tag
    [Browse(Never)] private     Tags            withoutAllTags;             //  32  entity must not have all tags
    [Browse(Never)] private     Tags            withoutAnyTags;             //  32  entity must not have any tag
         
    [Browse(Never)] private     ComponentTypes  allComponents;              //  32  entity must have all component types
    [Browse(Never)] private     ComponentTypes  anyComponents;              //  32  entity must have any component types
    [Browse(Never)] private     ComponentTypes  withoutAllComponents;       //  32  entity must not have all component types
    [Browse(Never)] private     ComponentTypes  withoutAnyComponents;       //  32  entity must not have any component types
                    
    [Browse(Never)] private     int             withoutAllTagsCount;        //   8
    [Browse(Never)] private     int             anyTagsCount;               //   8
    [Browse(Never)] private     int             allTagsCount;               //   8
        
    [Browse(Never)] private     int             withoutAllComponentsCount;  //   8
    [Browse(Never)] private     int             anyComponentsCount;         //   8
    [Browse(Never)] private     int             allComponentsCount;         //   8
    
    private                     bool            withoutDisabled;            //   1  if true (default) entity must be enabled
    internal                    int             version;                    //   4  incremented if filter changes
    private                     bool            frozen;                     //   1  if true the filter cannot be changed anymore
                    
    internal       ReadOnlyList<ValueCondition> valueConditions;            //  16
    #endregion
    
    
#region constructors
    /// <summary> Create a filter returning all <see cref="Entity.Enabled"/> entities. </summary>
    public QueryFilter() {
        withoutDisabled = true;
        withoutAnyTags  = EntityUtils.Disabled;
        Condition       = new FilterCondition(this); 
        valueConditions = new ReadOnlyList<ValueCondition>(Array.Empty<ValueCondition>());
    }
    
    internal QueryFilter(in Tags allTags) {
        this.allTags    = allTags;
        Condition       = new FilterCondition(this);
        valueConditions = new ReadOnlyList<ValueCondition>(Array.Empty<ValueCondition>());
    }
    #endregion

    /// <summary>
    /// <see cref="QueryFilter"/> cannot be changed anymore.
    /// </summary>
    public QueryFilter FreezeFilter() {
        frozen = true;
        return this;
    }
    
#region change tags filter
    /// <summary> Include entities containing all specified <paramref name="tags"/>. </summary>
    public QueryFilter AllTags(in Tags tags) {
        Change();
        allTags          = tags;
        allTagsCount     = tags.Count;
        return this;
    }
    
    /// <summary> Include entities containing any of the specified <paramref name="tags"/>. </summary>
    public QueryFilter AnyTags(in Tags tags) {
        Change();
        anyTags          = tags;
        anyTagsCount     = tags.Count;
        return this;
    }
    
    /// <summary> Exclude entities containing all specified <paramref name="tags"/>. </summary>
    public QueryFilter WithoutAllTags(in Tags tags) {
        Change();
        withoutAllTags       = tags;
        withoutAllTagsCount  = tags.Count;
        return this;
    }
    
    /// <summary> Exclude entities containing any of the specified <paramref name="tags"/>. </summary>
    public QueryFilter WithoutAnyTags(in Tags tags) {
        Change();
        withoutAnyTags       = tags;
        if (withoutDisabled) {
            withoutAnyTags.Add(EntityUtils.Disabled);
        }
        return this;
    }
    
    /// <summary> A query will return <see cref="Entity.Enabled"/> as well as disabled entities. </summary>
    public QueryFilter WithDisabled() {
        Change();
        withoutDisabled = false;
        withoutAnyTags.Remove(EntityUtils.Disabled);
        return this;
    }
    #endregion

    
#region change components filter
    /// <summary> Include entities containing all specified component <paramref name="types"/>. </summary>
    public QueryFilter AllComponents(in ComponentTypes types) {
        Change();
        allComponents        = types;
        allComponentsCount   = types.Count;
        return this;
    }
        
    /// <summary> Include entities containing any of the specified component <paramref name="types"/>. </summary>
    public QueryFilter AnyComponents(in ComponentTypes types) {
        Change();
        anyComponents        = types;
        anyComponentsCount   = types.Count;
        return this;
    }
        
    /// <summary> Exclude entities containing all specified component <paramref name="types"/>. </summary>
    public QueryFilter WithoutAllComponents(in ComponentTypes types) {
        Change();
        withoutAllComponents         = types;
        withoutAllComponentsCount    = types.Count;
        return this;
    }
     
    /// <summary> Exclude entities containing any of the specified  component <paramref name="types"/>. </summary>
    public QueryFilter WithoutAnyComponents(in ComponentTypes types) {
        Change();
        withoutAnyComponents         = types;
        return this;
    }
    #endregion
    
#region value condition filters
    /// <summary>
    /// Include entities having a component with the specified value.<br/>
    /// Executes in O(1).
    /// </summary>
    public QueryFilter HasValue<TComponent, TValue>(TValue value) where TComponent : struct, IIndexedComponent<TValue> {
        AddValueCondition(new HasValueCondition<TComponent, TValue>(value));
        return this;
    }
    
    /// <summary>
    /// Include entities having a component value in the specified range.<br/>
    /// Executes O(N ⋅ log N) N: all unique values.
    /// </summary>
    public QueryFilter ValueInRange<TComponent, TValue>(TValue min, TValue max) where TComponent : struct, IIndexedComponent<TValue> where TValue : IComparable<TValue> {
        AddValueCondition(new ValueInRangeCondition<TComponent, TValue>(min, max));
        return this;
    }
    
    private void AddValueCondition(ValueCondition valueCondition) {
        Change();
        valueConditions.Add(valueCondition);
    }
    #endregion
    
    
    /// <remarks>
    /// Reset <see cref="ArchetypeQuery.lastArchetypeCount"/> to force update of <see cref="ArchetypeQuery.archetypes"/>
    /// on subsequent call to <see cref="ArchetypeQuery.Archetypes"/>
    /// </remarks>
    private void Change() {
        if (frozen) throw new InvalidOperationException("QueryFilter was frozen and cannot be changed anymore.");
        version++;    
    }
    
#region filtering
    internal bool IsTagsMatch(in Tags tags)
    {
        if (anyTagsCount > 0)
        {
            if (!tags.HasAny(anyTags))
            {
                if (allTagsCount == 0) {
                    return false;
                }
                if (!tags.HasAll(allTags)) {
                    return false;
                }
            }
        } else {
            if (!tags.HasAll(allTags)) {
                return false;
            }
        }
        if (tags.HasAny(withoutAnyTags)) {
            return false;
        }
        if (withoutAllTagsCount > 0 && tags.HasAll(withoutAllTags)) {
            return false;
        }
        return true;
    }
    
    internal bool IsComponentsMatch(in ComponentTypes componentTypes)
    {
        if (anyComponentsCount > 0)
        {
            if (!componentTypes.HasAny(anyComponents))
            {
                if (allComponentsCount == 0) {
                    return false;
                }
                if (!componentTypes.HasAll(allComponents)) {
                    return false;
                }
            }
        } else {
            if (!componentTypes.HasAll(allComponents)) {
                return false;
            }
        }
        if (componentTypes.HasAny(withoutAnyComponents)) {
            return false;
        }
        if (withoutAllComponentsCount > 0 && componentTypes.HasAll(withoutAllComponents)) {
            return false;
        }
        return true;
    }
    #endregion
}

