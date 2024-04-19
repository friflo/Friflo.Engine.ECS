// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Contains component and tags filters added to an <see cref="ArchetypeQuery"/>. 
/// </summary>
public struct QueryFilter
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
    [Browse(Never)] internal    Tags            allTags;                    //  32  entity must have all tags
    [Browse(Never)] internal    Tags            anyTags;                    //  32  entity must have any tag
    [Browse(Never)] internal    Tags            withoutAllTags;             //  32  entity must not have all tags
    [Browse(Never)] internal    Tags            withoutAnyTags;             //  32  entity must not have any tag
         
    [Browse(Never)] internal    ComponentTypes  allComponents;              //  32  entity must have all component types
    [Browse(Never)] internal    ComponentTypes  anyComponents;              //  32  entity must have any component types
    [Browse(Never)] internal    ComponentTypes  withoutAllComponents;       //  32  entity must not have all component types
    [Browse(Never)] internal    ComponentTypes  withoutAnyComponents;       //  32  entity must not have any component types
                    
    [Browse(Never)] internal    int             withoutAllTagsCount;        //   8
    [Browse(Never)] internal    int             anyTagsCount;               //   8
    [Browse(Never)] internal    int             allTagsCount;               //   8
        
    [Browse(Never)] internal    int             withoutAllComponentsCount;  //   8
    [Browse(Never)] internal    int             anyComponentsCount;         //   8
    [Browse(Never)] internal    int             allComponentsCount;         //   8
    
                    internal    bool            withoutDisabled;            //   1  if true (default) entity must be enabled
    #endregion
    
    
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
}

