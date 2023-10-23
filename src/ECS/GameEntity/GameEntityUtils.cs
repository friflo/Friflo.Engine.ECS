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
        // --- add components
        var heaps       = archetype.Heaps;
        for (int n = 0; n < count; n++) {
            components[n] = heaps[n].GetComponentDebug(entity.compIndex); 
        }
        return components;
    }
    
    // ---------------------------------- Behavior utils ----------------------------------
    private  static readonly object[]   EmptyStructComponents   = Array.Empty<object>();
    private  static readonly Behavior[] EmptyBehaviors          = Array.Empty<Behavior>();
    internal const  int                 NoBehaviors             = -1;  
    
    internal static Exception MissingAttributeException(Type type) {
        var msg = $"Missing attribute [Behavior(\"<key>\")] on type: {type.Namespace}.{type.Name}";
        return new InvalidOperationException(msg);
    }

    internal static Behavior[] GetBehaviors(GameEntity entity) {
        if (entity.behaviorIndex == NoBehaviors) {
            return EmptyBehaviors;
        }
        return entity.archetype.gameEntityStore.GetBehaviors(entity);
    }
    
    internal static Behavior GetBehavior(GameEntity entity, Type behaviorType)
    {
        if (entity.behaviorIndex == NoBehaviors) {
            return null;
        }
        return entity.archetype.gameEntityStore.GetBehavior(entity, behaviorType);
    }
}