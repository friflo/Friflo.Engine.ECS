// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public static class GameEntityUtils
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
    
    internal static void AppendClassComponent<T>(GameEntity entity, T component)
        where T : ClassComponent
    {
        component.entity        = entity;
        ref var classComponents = ref entity.classComponents;
        var len                 = classComponents.Length;
        Utils.Resize(ref classComponents, len + 1);
        classComponents[len] = component;
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
}