// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Relations;

internal static class RelationUtils
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    internal static GetRelationKey<TComponent, TKey> CreateGetRelationKey<TComponent, TKey>()
        where TComponent : struct, IComponent
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(RelationUtils).GetMethod(nameof(GetRelationComponentKey), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(TComponent), typeof(TKey));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetRelationKey<TComponent,TKey>), genericMethod);
        return (GetRelationKey<TComponent,TKey>)genericDelegate;
    }
    
    private static TKey GetRelationComponentKey<TComponent,TKey>(in TComponent component)
        where TComponent : struct, IRelationComponent<TKey>
    {
        return component.GetRelationKey();
    }
}

internal static class RelationUtils<TComponent, TKey>
    where TComponent : struct, IComponent
{
    /// <summary> Returns the component value without boxing. </summary>
    internal static readonly GetRelationKey<TComponent, TKey> GetRelationKey;
        
    static RelationUtils() {
        GetRelationKey = RelationUtils.CreateGetRelationKey<TComponent,TKey>();
    }
}
    
internal delegate TKey GetRelationKey<TComponent, out TKey>(in TComponent component)
    where TComponent : struct, IComponent;
