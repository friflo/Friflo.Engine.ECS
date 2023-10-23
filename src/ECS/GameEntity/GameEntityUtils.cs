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
        return entity.archetype.ComponentCount + entity.ClassComponents.Length;
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
            var classComponents = GetClassComponents(entity);
            foreach (var refComp in classComponents) {
                sb.Append('*');
                sb.Append(refComp.GetType().Name);
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
    private  static readonly object[]           EmptyStructComponents   = Array.Empty<object>();
    private  static readonly ClassComponent[]   EmptyClassComponents    = Array.Empty<ClassComponent>();
    internal const  int                         NoBehaviors             = -1;  
    
    private static Exception MissingAttributeException(Type type) {
        var msg = $"Missing attribute [ClassComponent(\"<key>\")] on type: {type.Namespace}.{type.Name}";
        return new InvalidOperationException(msg);
    }

    internal static ClassComponent[] GetClassComponents(GameEntity entity) {
        if (entity.behaviorIndex == NoBehaviors) {
            return EmptyClassComponents;
        }
        return entity.archetype.gameEntityStore.entityBehaviors[entity.behaviorIndex].classComponents;
    }
    
    internal static ClassComponent GetClassComponent(GameEntity entity, Type classType)
    {
        if (entity.behaviorIndex == NoBehaviors) {
            return null;
        }
        var classComponents = entity.archetype.gameEntityStore.entityBehaviors[entity.behaviorIndex].classComponents;
        foreach (var component in classComponents) {
            if (component.GetType() == classType) {
                return component;
            }
        }
        return null;
    }
    
    internal static void AppendClassComponent<T>(GameEntity entity, T component)
        where T : ClassComponent
    {
        component.entity    = entity;
        var store           = entity.archetype.gameEntityStore;
        if (entity.behaviorIndex == NoBehaviors) {
            // case: entity has not behaviors => add new Behaviors entry
            var lastIndex = entity.behaviorIndex = store.entityBehaviorCount++;
            if (store.entityBehaviors.Length == lastIndex) {
                Utils.Resize(ref store.entityBehaviors, 2 * lastIndex);
            }
            store.entityBehaviors[lastIndex] = new Behaviors(entity.id, new ClassComponent[] { component });
        } else {
            // case: entity already has behaviors => add component to its behaviors
            ref var classComponents = ref store.entityBehaviors[entity.behaviorIndex].classComponents;
            var len                 = classComponents.Length;
            Utils.Resize(ref classComponents, len + 1);
            classComponents[len] = component;
        }
    }
    
    internal static ClassComponent AddClassComponent(GameEntity entity, ClassComponent component, Type classType, int classIndex)
    {
        if (classIndex == ClassUtils.MissingAttribute) {
            throw MissingAttributeException(classType);
        }
        if (component.entity != null) {
            throw new InvalidOperationException("component already added to an entity");
        }
        component.entity    = entity;
        var store           = entity.archetype.gameEntityStore;
        if (entity.behaviorIndex == NoBehaviors) {
            // case: entity has not behaviors => add new Behaviors entry
            var lastIndex = entity.behaviorIndex = store.entityBehaviorCount++;
            if (store.entityBehaviors.Length == lastIndex) {
                Utils.Resize(ref store.entityBehaviors, 2 * lastIndex);
            }
            store.entityBehaviors[lastIndex] = new Behaviors(entity.id, new ClassComponent [] { component });
            return null;
        }
        // case: entity has already behaviors => add component to its behaviors
        ref var behaviors   = ref store.entityBehaviors[entity.behaviorIndex];
        var classes         = behaviors.classComponents;
        var len             = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var current = classes[n]; 
            if (current.GetType() == classType) {
                classes[n] = component;
                current.entity = null;
                return component;
            }
        }
        // --- case: map does not contain a component Type
        Utils.Resize(ref behaviors.classComponents, len + 1);
        behaviors.classComponents[len] = component;
        return null;
    }
    
    internal static ClassComponent RemoveClassComponent(GameEntity entity, Type classType)
    {
        if (entity.behaviorIndex == NoBehaviors) {
            return null;
        }
        var store           = entity.archetype.gameEntityStore;
        ref var behaviors   = ref store.entityBehaviors[entity.behaviorIndex];
        var classes         = behaviors.classComponents;
        var len             = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var classComponent = classes[n];
            if (classComponent.GetType() == classType)
            {
                // case: found behavior in entity behaviors
                classComponent.entity   = null;
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
                    return classComponent;
                }
                // case: entity has two or more behaviors. Remove the given one from its behaviors
                var classComponents = new ClassComponent[len - 1];
                for (int i = 0; i < n; i++) {
                    classComponents[i]     = classes[i];
                }
                for (int i = n + 1; i < len; i++) {
                    classComponents[i - 1] = classes[i];
                }
                behaviors.classComponents = classComponents;
                return classComponent;
            }
        }
        return null;
    }
}