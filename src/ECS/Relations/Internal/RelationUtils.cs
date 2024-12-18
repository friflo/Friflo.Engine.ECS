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
    internal static GetRelationKey<TRelation, TKey> CreateGetRelationKey<TRelation, TKey>()
        where TRelation : struct, IRelation
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(RelationUtils).GetMethod(nameof(GetRelationKey), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(TRelation), typeof(TKey));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetRelationKey<TRelation,TKey>), genericMethod);
        return (GetRelationKey<TRelation,TKey>)genericDelegate;
    }
    
    private static TKey GetRelationKey<TRelation,TKey>(in TRelation component)
        where TRelation : struct, IRelation<TKey>
    {
        return component.GetRelationKey();
    }
}

internal static class RelationUtils<TRelation, TKey>
    where TRelation : struct, IRelation
{
    /// <summary> Returns the component value without boxing. </summary>
    internal static readonly GetRelationKey<TRelation, TKey> GetRelationKey;
        
    static RelationUtils() {
        GetRelationKey = RelationUtils.CreateGetRelationKey<TRelation,TKey>();
    }
}
    
internal delegate TKey GetRelationKey<TRelation, out TKey>(in TRelation component)
    where TRelation : struct, IRelation;
