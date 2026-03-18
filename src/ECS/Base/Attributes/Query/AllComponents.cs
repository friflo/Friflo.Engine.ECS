// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UnusedTypeParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1> : Attribute
    where T1 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4, T5> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4, T5, T6> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4, T5, T6, T7> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
    where T8 : struct, IComponent
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
    where T8 : struct, IComponent
    where T9 : struct, IComponent
{ }


[AttributeUsage(AttributeTargets.Method)] public sealed class AllComponentsAttribute<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Attribute
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
    where T8 : struct, IComponent
    where T9 : struct, IComponent
    where T10 : struct, IComponent
{ }