// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal static class RelationUtils
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "TODO")] // TODO
    internal static GetRelationValue<TComponent,TValue> CreateGetValue<TComponent,TValue>() where TComponent : struct, IComponent
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        var method          = typeof(RelationUtils).GetMethod(nameof(GetRelationComponentValue), flags);
        var genericMethod   = method!.MakeGenericMethod(typeof(TComponent), typeof(TValue));
        
        var genericDelegate = Delegate.CreateDelegate(typeof(GetRelationValue<TComponent,TValue>), genericMethod);
        return (GetRelationValue<TComponent,TValue>)genericDelegate;
    }
    
    private static TValue GetRelationComponentValue<TComponent,TValue>(in TComponent component) where TComponent : struct, IRelationComponent<TValue> {
        return component.GetRelation();
    }
}

internal static class RelationUtils<TComponent, TValue>  where TComponent : struct, IComponent
{
    /// <summary> Returns the component value without boxing. </summary>
    internal static readonly GetRelationValue<TComponent,TValue> GetRelationValue;
        
    static RelationUtils() {
        GetRelationValue = RelationUtils.CreateGetValue<TComponent,TValue>();
    }
}
    
internal delegate TValue GetRelationValue<TComponent, out TValue>(in TComponent component) where TComponent : struct, IComponent;
