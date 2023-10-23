// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public partial class GameEntityStore
{
    // --------------------------------- behavior methods ---------------------------------
    internal Behavior[] GetBehaviors(GameEntity entity) {
        return entityBehaviors[entity.behaviorIndex].classes;
    }
    
    internal Behavior GetBehavior(GameEntity entity, Type behaviorType)
    {
        var classes = entityBehaviors[entity.behaviorIndex].classes;
        foreach (var behavior in classes) {
            if (behavior.GetType() == behaviorType) {
                return behavior;
            }
        }
        return null;
    }
    
    internal void AppendBehavior(GameEntity entity, Behavior behavior)
    {
        behavior.entity = entity;
        if (entity.behaviorIndex == GameEntityUtils.NoBehaviors) {
            // case: entity has not behaviors => add new Behaviors entry
            var lastIndex = entity.behaviorIndex = entityBehaviorCount++;
            if (entityBehaviors.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                Utils.Resize(ref entityBehaviors, newLength);
            }
            entityBehaviors[lastIndex] = new Behaviors(entity.id, new Behavior[] { behavior });
        } else {
            // case: entity already has behaviors => add behavior to its behaviors
            ref var classes = ref entityBehaviors[entity.behaviorIndex].classes;
            var len = classes.Length;
            Utils.Resize(ref classes, len + 1);
            classes[len] = behavior;
        }
    }
    
    internal Behavior AddBehavior(GameEntity entity, Behavior behavior, Type behaviorType, int behaviorIndex)
    {
        behavior.entity = entity;
        if (entity.behaviorIndex == GameEntityUtils.NoBehaviors)
        {
            // case: entity has not behaviors => add new Behaviors entry
            var lastIndex = entity.behaviorIndex = entityBehaviorCount++;
            if (entityBehaviors.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                Utils.Resize(ref entityBehaviors, newLength);
            }
            entityBehaviors[lastIndex] = new Behaviors(entity.id, new Behavior [] { behavior });
            return null;
        }
        // case: entity has already behaviors => add / replace behavior to / in behaviors
        ref var entityBehavior  = ref entityBehaviors[entity.behaviorIndex];
        var classes             = entityBehavior.classes;
        var len                 = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var current = classes[n]; 
            if (current.GetType() == behaviorType) {
                // case: behaviors contains a behavior of the given behaviorType => replace current behavior
                classes[n] = behavior;
                current.entity = null;
                return behavior;
            }
        }
        // --- case: behaviors does not contain a behavior of the given behaviorType => add behavior
        Utils.Resize(ref entityBehavior.classes, len + 1);
        entityBehavior.classes[len] = behavior;
        return null;
    }
    
    internal Behavior RemoveBehavior(GameEntity entity, Type behaviorType)
    {
        ref var entityBehavior  = ref entityBehaviors[entity.behaviorIndex];
        var classes             = entityBehavior.classes;
        var len                 = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var behavior = classes[n];
            if (behavior.GetType() != behaviorType) {
                continue;
            }
            // case: found behavior in entity behaviors
            behavior.entity   = null;
            if (len == 1) {
                // case: behavior is the only one attached to the entity => remove complete behaviors entry 
                var lastIndex       = --entityBehaviorCount;
                var lastEntityId    = entityBehaviors[lastIndex].id;
                // Is the Behavior not the last in store.entityBehaviors?
                if (entity.id != lastEntityId) {
                    // move behaviorIndex of last item in store.entityBehaviors to the index which will be removed
                    entityBehaviors[entity.behaviorIndex] = entityBehaviors[lastIndex];
                    nodes[lastEntityId].entity.behaviorIndex = entity.behaviorIndex;
                }
                entityBehaviors[lastIndex] = default;               // clear last Behavior entry
                entity.behaviorIndex = GameEntityUtils.NoBehaviors; // set entity state to: contains no behaviors 
                return behavior;
            }
            // case: entity has two or more behaviors. Remove the given one from its behaviors
            var behaviors = new Behavior[len - 1];
            for (int i = 0; i < n; i++) {
                behaviors[i]     = classes[i];
            }
            for (int i = n + 1; i < len; i++) {
                behaviors[i - 1] = classes[i];
            }
            entityBehavior.classes = behaviors;
            return behavior;
        }
        return null;
    }
}
