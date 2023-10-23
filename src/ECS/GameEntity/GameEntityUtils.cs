﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
using static Friflo.Fliox.Engine.ECS.StructInfo;

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
    internal static readonly int                BehaviorsIndex          = StructHeap<Behaviors>.StructIndex;
    
    private static Exception MissingAttributeException(Type type) {
        var msg = $"Missing attribute [ClassComponent(\"<key>\")] on type: {type.Namespace}.{type.Name}";
        return new InvalidOperationException(msg);
    }

    internal static ClassComponent[] GetClassComponents(GameEntity entity) {
        var heap = (StructHeap<Behaviors>)entity.archetype.heapMap[BehaviorsIndex];
        if (heap == null) {
            return EmptyClassComponents;
        }
        return heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize].classComponents;
    }
    
    internal static ClassComponent GetClassComponent(GameEntity entity, Type classType)
    {
        var heap = (StructHeap<Behaviors>)entity.archetype.heapMap[BehaviorsIndex];
        if (heap == null) {
            return null;
        }
        var classComponents = heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize].classComponents;
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
        component.entity = entity;
        var heap = (StructHeap<Behaviors>)entity.archetype.heapMap[BehaviorsIndex];
        if (heap == null) {
            var classComponents = new ClassComponent[] { component };
            entity.AddComponent(new Behaviors(classComponents));
        } else {
            ref var classComponents = ref heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize].classComponents;
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
        var heap = (StructHeap<Behaviors>)entity.archetype.heapMap[BehaviorsIndex];
        if (heap == null) {
            var classComponents = new [] { component };
            entity.AddComponent(new Behaviors(classComponents));
            return null;
        }
        ref var behaviors   = ref heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize];
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
        var heap = (StructHeap<Behaviors>)entity.archetype.heapMap[BehaviorsIndex];
        if (heap == null) {
            return null;
        }
        ref var behaviors   = ref heap.chunks[entity.compIndex / ChunkSize].components[entity.compIndex % ChunkSize];
        var classes         = behaviors.classComponents;
        var len             = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var classComponent = classes[n];
            if (classComponent.GetType() == classType)
            {
                if (len == 1) {
                    classComponent.entity   = null;
                    entity.RemoveComponent<Behaviors>();
                    return classComponent;
                }
                var classComponents = new ClassComponent[len - 1];
                for (int i = 0; i < n; i++) {
                    classComponents[i]     = classes[i];
                }
                for (int i = n + 1; i < len; i++) {
                    classComponents[i - 1] = classes[i];
                }
                classComponent.entity       = null;
                behaviors.classComponents   = classComponents;
                return classComponent;
            }
        }
        return null;
    }
}