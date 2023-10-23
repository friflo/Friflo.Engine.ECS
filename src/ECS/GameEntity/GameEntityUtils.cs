// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal static class GameEntityExtensions
{
    internal static int ComponentCount (this GameEntity entity) {
        return entity.archetype.ComponentCount + entity.Behaviors.Length;
    }
}
    
    
internal static class GameEntityUtils
{
    internal static string GameEntityToString(GameEntity entity, StringBuilder sb)
    {
        var archetype = entity.archetype;
        sb.Append("id: ");
        sb.Append(entity.id);
        if (archetype == null) {
            sb.Append("  (detached)");
            return sb.ToString();
        }
        if (entity.HasName) {
            var name = entity.Name.Value;
            if (name != null) {
                sb.Append("  \"");
                sb.Append(name);
                sb.Append('\"');
                return sb.ToString();
            }
        }
        if (entity.ComponentCount() == 0) {
            sb.Append("  []");
        } else {
            sb.Append("  [");
            var behaviors = GetBehaviors(entity);
            foreach (var behavior in behaviors) {
                sb.Append('*');
                sb.Append(behavior.GetType().Name);
                sb.Append(", ");
            }
            foreach (var heap in archetype.Heaps) {
                sb.Append(heap.StructType.Name);
                sb.Append(", ");
            }
            sb.Length -= 2;
            sb.Append(']');
        }
        return sb.ToString();
    }
    
    internal static object[] GetComponentsDebug(GameEntity entity)
    {
        var archetype   = entity.archetype;
        var count       = archetype.ComponentCount;
        if (count == 0) {
            return EmptyStructComponents;
        }
        var components  = new object[count];
        // --- add struct components
        var heaps       = archetype.Heaps;
        for (int n = 0; n < count; n++) {
            components[n] = heaps[n].GetComponentDebug(entity.compIndex); 
        }
        return components;
    }
    
    // ---------------------------------- ClassComponent utils ----------------------------------
    private  static readonly object[]   EmptyStructComponents   = Array.Empty<object>();
    private  static readonly Behavior[] EmptyBehaviors          = Array.Empty<Behavior>();
    internal const  int                 NoBehaviors             = -1;  
    
    private static Exception MissingAttributeException(Type type) {
        var msg = $"Missing attribute [ClassComponent(\"<key>\")] on type: {type.Namespace}.{type.Name}";
        return new InvalidOperationException(msg);
    }

    internal static Behavior[] GetBehaviors(GameEntity entity) {
        if (entity.behaviorIndex == NoBehaviors) {
            return EmptyBehaviors;
        }
        return entity.archetype.gameEntityStore.entityBehaviors[entity.behaviorIndex].classes;
    }
    
    internal static Behavior GetBehavior(GameEntity entity, Type behaviorType)
    {
        if (entity.behaviorIndex == NoBehaviors) {
            return null;
        }
        var classes = entity.archetype.gameEntityStore.entityBehaviors[entity.behaviorIndex].classes;
        foreach (var behavior in classes) {
            if (behavior.GetType() == behaviorType) {
                return behavior;
            }
        }
        return null;
    }
    
    internal static void AppendClassComponent<T>(GameEntity entity, T behavior)
        where T : Behavior
    {
        behavior.entity    = entity;
        var store           = entity.archetype.gameEntityStore;
        if (entity.behaviorIndex == NoBehaviors) {
            // case: entity has not behaviors => add new Behaviors entry
            var lastIndex = entity.behaviorIndex = store.entityBehaviorCount++;
            if (store.entityBehaviors.Length == lastIndex) {
                Utils.Resize(ref store.entityBehaviors, 2 * lastIndex);
            }
            store.entityBehaviors[lastIndex] = new Behaviors(entity.id, new Behavior[] { behavior });
        } else {
            // case: entity already has behaviors => add component to its behaviors
            ref var classes = ref store.entityBehaviors[entity.behaviorIndex].classes;
            var len = classes.Length;
            Utils.Resize(ref classes, len + 1);
            classes[len] = behavior;
        }
    }
    
    internal static Behavior AddBehavior(GameEntity entity, Behavior behavior, Type behaviorType, int behaviorIndex)
    {
        if (behaviorIndex == ClassUtils.MissingAttribute) {
            throw MissingAttributeException(behaviorType);
        }
        if (behavior.entity != null) {
            throw new InvalidOperationException("component already added to an entity");
        }
        behavior.entity = entity;
        var store       = entity.archetype.gameEntityStore;
        if (entity.behaviorIndex == NoBehaviors) {
            // case: entity has not behaviors => add new Behaviors entry
            var lastIndex = entity.behaviorIndex = store.entityBehaviorCount++;
            if (store.entityBehaviors.Length == lastIndex) {
                Utils.Resize(ref store.entityBehaviors, 2 * lastIndex);
            }
            store.entityBehaviors[lastIndex] = new Behaviors(entity.id, new Behavior [] { behavior });
            return null;
        }
        // case: entity has already behaviors => add component to its behaviors
        ref var entityBehavior  = ref store.entityBehaviors[entity.behaviorIndex];
        var classes             = entityBehavior.classes;
        var len                 = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var current = classes[n]; 
            if (current.GetType() == behaviorType) {
                classes[n] = behavior;
                current.entity = null;
                return behavior;
            }
        }
        // --- case: map does not contain a component Type
        Utils.Resize(ref entityBehavior.classes, len + 1);
        entityBehavior.classes[len] = behavior;
        return null;
    }
    
    internal static Behavior RemoveBehavior(GameEntity entity, Type behaviorType)
    {
        if (entity.behaviorIndex == NoBehaviors) {
            return null;
        }
        var store               = entity.archetype.gameEntityStore;
        ref var entityBehavior  = ref store.entityBehaviors[entity.behaviorIndex];
        var classes             = entityBehavior.classes;
        var len                 = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var behavior = classes[n];
            if (behavior.GetType() == behaviorType)
            {
                // case: found behavior in entity behaviors
                behavior.entity   = null;
                if (len == 1) {
                    // case: behavior is the only one attached to the entity => remove complete behaviors entry 
                    var lastIndex       = --store.entityBehaviorCount;
                    var lastEntityId    = store.entityBehaviors[lastIndex].id;
                    store.entityBehaviors[lastIndex] = default;
                    // set behaviorIndex of last item in store.entityBehaviors to the index which will be removed
                    if (entity.id != lastEntityId) {
                        store.nodes[lastEntityId].entity.behaviorIndex = entity.behaviorIndex;
                    }
                    entity.behaviorIndex    = NoBehaviors;
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
        }
        return null;
    }
}