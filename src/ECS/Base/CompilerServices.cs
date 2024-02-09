
using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("Tests-internal")]
[assembly: InternalsVisibleTo("Fliox.Tests-internal")]

#if !NETCOREAPP3_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    // This is needed to enable following features in .NET framework and .NET core <= 3.1 projects:
    // - init only setter properties. See [Init only setters - C# 9.0 draft specifications | Microsoft Learn] https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init
    // - record types
    internal static class IsExternalInit { }
}

#endif

/*
 * C# preprocessor directives used by Friflo.Engine.ECS
 *
 * NETCOREAPP3_0_OR_GREATER - for init only setter properties
 *                          - System.Runtime.Intrinsics.Vector256[T]
 *                          - System.Numerics.BitOperations
 *
 * NET5_0_OR_GREATER        - System.Collections.Generic.IReadOnlySet[T]
 *
 * NET6_0_OR_GREATER        - System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault[TKey, TValue]
 *
 * NET7_0_OR_GREATER        - System.Runtime.Intrinsics.Vector256[T] operators
 */
