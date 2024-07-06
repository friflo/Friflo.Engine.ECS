// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Linq;
using System.Text;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public static class CollectionExtensions
{
    
    /// <summary>
    /// Returns a string containing the <see cref="object.ToString"/> for each component.<br/>
    /// E.g <c>"{ 1, 3, 7 }"</c>
    /// </summary>
    public static string Debug<T>(this IEnumerable<T> enumerable)
    {
        var array = enumerable.ToArray();
        if (array.Length == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var item in array) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(item);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    
    /// <summary>
    /// Returns a string containing the entity ids.<br/>
    /// E.g <c>"{ 1, 3, 7 }"</c>
    /// </summary>
    public static string Debug(this IEnumerable<Entity> entities)
    {
        var array = entities.ToArray();
        if (array.Length == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var entity in array) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(entity.Id);
        }
        sb.Append(" }");
        return sb.ToString();
    }
    
    /// <summary>
    /// Returns a string containing the <see cref="object.ToString"/> for each component.<br/>
    /// E.g <c>"{ 1, 3, 7 }"</c>
    /// </summary>
    public static string Debug<T>(this Chunk<T> chunk)
        where T : struct, IComponent
    {
        if (chunk.Length == 0) return "{ }";
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var item in chunk.Span) {
            if (sb.Length > 2) sb.Append(", ");
            sb.Append(item);
        }
        sb.Append(" }");
        return sb.ToString();
    }
}