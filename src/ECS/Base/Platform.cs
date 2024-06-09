// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class Platform
{
    internal static readonly bool IsUnityRuntime = GetIsUnityRuntime();
        
    private static bool GetIsUnityRuntime() {
#pragma warning disable RS0030
        return Type.GetType("UnityEngine.Application, UnityEngine") != null;
#pragma warning restore RS0030
    }
}