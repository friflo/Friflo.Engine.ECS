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
        return entity.archetype.structCount + entity.Scripts.Length;
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
            var name = entity.Name.value;
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
            var behaviors = GetScripts(entity);
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
    
    internal static IComponent[] GetComponentsDebug(GameEntity entity)
    {
        var archetype   = entity.archetype;
        var count       = archetype.structCount;
        if (count == 0) {
            return EmptyComponents;
        }
        var components  = new IComponent[count];
        // --- add components
        var heaps       = archetype.Heaps;
        for (int n = 0; n < count; n++) {
            components[n] = heaps[n].GetComponentDebug(entity.compIndex); 
        }
        return components;
    }
    
    // ---------------------------------- Script utils ----------------------------------
    private  static readonly IComponent[]   EmptyComponents = Array.Empty<IComponent>();
    private  static readonly Script[]       EmptyScripts  = Array.Empty<Script>();
    internal const  int                     NoScripts     = -1;  
    
    private  static Exception MissingAttributeException(Type type) {
        var msg = $"Missing attribute [Script(\"<key>\")] on type: {type.Namespace}.{type.Name}";
        return new InvalidOperationException(msg);
    }

    internal static Script[] GetScripts(GameEntity entity) {
        if (entity.behaviorIndex == NoScripts) {
            return EmptyScripts;
        }
        return entity.archetype.gameEntityStore.GetScripts(entity);
    }
    
    internal static Script GetScript(GameEntity entity, Type behaviorType)
    {
        if (entity.behaviorIndex == NoScripts) {
            return null;
        }
        return entity.archetype.gameEntityStore.GetScript(entity, behaviorType);
    }
    
    internal static Script AddScript(GameEntity entity, Script behavior, Type behaviorType, int classIndex)
    {
        if (classIndex == ClassUtils.MissingAttribute) {
            throw MissingAttributeException(behaviorType);
        }
        if (behavior.entity != null) {
            throw new InvalidOperationException($"behavior already added to an entity. current entity id: {behavior.entity.id}");
        }
        return entity.archetype.gameEntityStore.AddScript(entity, behavior, behaviorType);
    }
    
    internal static Script RemoveScript(GameEntity entity, Type behaviorType) {
        if (entity.behaviorIndex == NoScripts) {
            return null;
        }
        return entity.archetype.gameEntityStore.RemoveScript(entity, behaviorType);
    }
}