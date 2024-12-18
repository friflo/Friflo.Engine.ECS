using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal delegate void CopyScript<in TScript>(TScript value, TScript target);


internal static class CopyScriptUtils
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2090", Justification = "TODO")] // TODO
    internal static CopyScript<TScript> CreateCopyScript<TScript>()
    {
        const BindingFlags flags    = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod;
        var method = typeof(TScript).GetMethod("CopyScript", flags);
        if (method is null) {
            return null;
        }
        var genericDelegate = Delegate.CreateDelegate(typeof(CopyScript<TScript>), method);
        return (CopyScript<TScript>)genericDelegate;
    }
}

internal static class CopyScriptUtils<TValue>
{
    internal static readonly    CopyScript<TValue>    CopyScript    = CopyScriptUtils.CreateCopyScript<TValue>();
}