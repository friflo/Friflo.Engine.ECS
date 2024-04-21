// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Contains component and tags filters added to an <see cref="ArchetypeQuery"/>. 
/// </summary>
public class QueryFilter
{
#region properties
    /** Entity must have all tags. */               public    Tags            AllTags               => allTags;
    /** Entity must have any tag. */                public    Tags            AnyTags               => anyTags;
    /** Entity must not have all tags. */           public    Tags            WithoutAllTags        => withoutAllTags;
    /** Entity must not have any tag. */            public    Tags            WithoutAnyTags        => withoutAnyTags;
                        
    /** Entity must have all component types. */    public    ComponentTypes  AllComponents         => allComponents;
    /** Entity must have any component types. */    public    ComponentTypes  AnyComponents         => anyComponents;
    /** Entity must not have all component types. */public    ComponentTypes  WithoutAllComponents  => withoutAllComponents;
    /** Entity must not have any component types. */public    ComponentTypes  WithoutAnyComponents  => withoutAnyComponents;
    #endregion

#region fields
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
    
                    private     bool            withoutDisabled;            //   1  if true (default) entity must be enabled
                    internal    int             version;                    //   4  incremented if filter changes
    #endregion
    
    
#region constructors
    public QueryFilter() {
        withoutDisabled  = true;
        withoutAnyTags   = EntityUtils.Disabled;
    }
    
    internal QueryFilter(in Tags allTags) {
        this.allTags = allTags;
    }
    #endregion


#region change tags filter
    public void SetHasAllTags(in Tags tags) {
        allTags          = tags;
        allTagsCount     = tags.Count;
        Changed();
    }
    
    public void SetHasAnyTags(in Tags tags) {
        anyTags          = tags;
        anyTagsCount     = tags.Count;
        Changed();
    }
    
    public void SetWithoutAllTags(in Tags tags) {
        withoutAllTags       = tags;
        withoutAllTagsCount  = tags.Count;
        Changed();
    }
    
    public void SetWithoutAnyTags(in Tags tags) {
        withoutAnyTags       = tags;
        if (withoutDisabled) {
            withoutAnyTags.Add(EntityUtils.Disabled);
        }
        Changed();
    }
    
    public void SetWithDisabled() {
        withoutDisabled = false;
        withoutAnyTags.Remove(EntityUtils.Disabled);
        Changed();
    }
    #endregion

    
#region change components filter
    public void SetHasAllComponents(in ComponentTypes types) {
        allComponents        = types;
        allComponentsCount   = types.Count;
        Changed();
    }
        
    public void SetHasAnyComponents(in ComponentTypes types) {
        anyComponents        = types;
        anyComponentsCount   = types.Count;
        Changed();
    }
        
    public void SetWithoutAllComponents(in ComponentTypes types) {
        withoutAllComponents         = types;
        withoutAllComponentsCount    = types.Count;
        Changed();
    }
        
    public void SetWithoutAnyComponents(in ComponentTypes types) {
        withoutAnyComponents         = types;
        Changed();
    }
    #endregion
    
    
    /// <remarks>
    /// Reset <see cref="ArchetypeQuery.lastArchetypeCount"/> to force update of <see cref="ArchetypeQuery.archetypes"/>
    /// on subsequent call to <see cref="ArchetypeQuery.Archetypes"/>
    /// </remarks>
    private void Changed() {
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

