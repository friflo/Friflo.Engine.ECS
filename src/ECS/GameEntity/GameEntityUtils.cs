// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

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
        if (entity.ComponentCount == 0) {
            sb.Append("  []");
        } else {
            sb.Append("  [");
            foreach (var refComp in entity.classComponents) {
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
        var objects = new object[entity.ComponentCount];
        // --- add struct components
        var heaps       = entity.archetype.Heaps;
        var count       = heaps.Length;
        for (int n = 0; n < count; n++) {
            objects[n] = heaps[n].GetComponentDebug(entity.compIndex); 
        }
        // --- add class components
        foreach (var component in entity.classComponents) {
            objects[count++] = component;
        }
        return objects;
    }
    
    // ---------------------------------- ClassComponent utils ----------------------------------
    internal static readonly ClassComponent[] EmptyComponents   = Array.Empty<ClassComponent>();
    
    internal static void AppendClassComponent<T>(GameEntity entity, T component)
        where T : ClassComponent
    {
        component.entity        = entity;
        ref var classComponents = ref entity.classComponents;
        var len                 = classComponents.Length;
        Utils.Resize(ref classComponents, len + 1);
        classComponents[len] = component;
    }
    
    private static Exception MissingAttributeException(Type type) {
        var msg = $"Missing attribute [ClassComponent(\"<key>\")] on type: {type.Namespace}.{type.Name}";
        return new InvalidOperationException(msg);
    }
    
    internal static ClassComponent GetClassComponent(GameEntity entity, Type classType)
    {
        foreach (var component in entity.classComponents) {
            if (component.GetType() == classType) {
                return component;
            }
        }
        return null;
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
        var classes         = entity.classComponents;
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
        Utils.Resize(ref entity.classComponents, len + 1);
        entity.classComponents[len] = component;
        return null;
    }
    
    internal static ClassComponent RemoveClassComponent(GameEntity entity, Type classType)
    {
        var classes = entity.classComponents;
        var len     = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var classComponent = classes[n];
            if (classComponent.GetType() == classType)
            {
                var classComponents = new ClassComponent[len - 1];
                for (int i = 0; i < n; i++) {
                    classComponents[i]     = classes[i];
                }
                for (int i = n + 1; i < len; i++) {
                    classComponents[i - 1] = classes[i];
                }
                classComponent.entity   = null;
                entity.classComponents  = classComponents;
                return classComponent;
            }
        }
        return null;
    }
}