// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public partial class GameEntityStore
{
    // --------------------------------- behavior methods ---------------------------------
    internal Script[] GetScripts(GameEntity entity) {
        return entityScripts[entity.behaviorIndex].classes;
    }
    
    internal Script GetScript(GameEntity entity, Type behaviorType)
    {
        var classes = entityScripts[entity.behaviorIndex].classes;
        foreach (var behavior in classes) {
            if (behavior.GetType() == behaviorType) {
                return behavior;
            }
        }
        return null;
    }
    
    internal void AppendScript(GameEntity entity, Script behavior)
    {
        behavior.entity = entity;
        if (entity.behaviorIndex == GameEntityUtils.NoScripts) {
            // case: entity has not behaviors => add new Scripts entry
            var lastIndex = entity.behaviorIndex = entityScriptCount++;
            if (entityScripts.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                Utils.Resize(ref entityScripts, newLength);
            }
            entityScripts[lastIndex] = new EntityScripts(entity.id, new Script[] { behavior });
        } else {
            // case: entity already has behaviors => add behavior to its behaviors
            ref var classes = ref entityScripts[entity.behaviorIndex].classes;
            var len = classes.Length;
            Utils.Resize(ref classes, len + 1);
            classes[len] = behavior;
        }
    }
    
    internal Script AddScript(GameEntity entity, Script behavior, Type behaviorType)
    {
        behavior.entity = entity;
        if (entity.behaviorIndex == GameEntityUtils.NoScripts)
        {
            // case: entity has not behaviors => add new Scripts entry
            var lastIndex = entity.behaviorIndex = entityScriptCount++;
            if (entityScripts.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                Utils.Resize(ref entityScripts, newLength);
            }
            entityScripts[lastIndex] = new EntityScripts(entity.id, new Script [] { behavior });
            return null;
        }
        // case: entity has already behaviors => add / replace behavior to / in behaviors
        ref var entityScript    = ref entityScripts[entity.behaviorIndex];
        var classes             = entityScript.classes;
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
        Utils.Resize(ref entityScript.classes, len + 1);
        entityScript.classes[len] = behavior;
        return null;
    }
    
    internal Script RemoveScript(GameEntity entity, Type behaviorType)
    {
        ref var entityScript    = ref entityScripts[entity.behaviorIndex];
        var classes             = entityScript.classes;
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
                var lastIndex       = --entityScriptCount;
                var lastEntityId    = entityScripts[lastIndex].id;
                // Is the Script not the last in store.entityScripts?
                if (entity.id != lastEntityId) {
                    // move behaviorIndex of last item in store.entityScripts to the index which will be removed
                    entityScripts[entity.behaviorIndex] = entityScripts[lastIndex];
                    nodes[lastEntityId].entity.behaviorIndex = entity.behaviorIndex;
                }
                entityScripts[lastIndex] = default;               // clear last Script entry
                entity.behaviorIndex = GameEntityUtils.NoScripts; // set entity state to: contains no behaviors 
                return behavior;
            }
            // case: entity has two or more behaviors. Remove the given one from its behaviors
            var behaviors = new Script[len - 1];
            for (int i = 0; i < n; i++) {
                behaviors[i]     = classes[i];
            }
            for (int i = n + 1; i < len; i++) {
                behaviors[i - 1] = classes[i];
            }
            entityScript.classes = behaviors;
            return behavior;
        }
        return null;
    }
}
