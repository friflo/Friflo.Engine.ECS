// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable UnusedTypeParameter
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1> : Attribute
    where T1 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4, T5> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
    where T5 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4, T5, T6> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
    where T5 : struct, ITag
    where T6 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4, T5, T6, T7> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
    where T5 : struct, ITag
    where T6 : struct, ITag
    where T7 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
    where T5 : struct, ITag
    where T6 : struct, ITag
    where T7 : struct, ITag
    where T8 : struct, ITag
{ }

[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
    where T5 : struct, ITag
    where T6 : struct, ITag
    where T7 : struct, ITag
    where T8 : struct, ITag
    where T9 : struct, ITag
{ }


[AttributeUsage(AttributeTargets.Method)] public sealed class WithoutAnyTagsAttribute<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Attribute
    where T1 : struct, ITag
    where T2 : struct, ITag
    where T3 : struct, ITag
    where T4 : struct, ITag
    where T5 : struct, ITag
    where T6 : struct, ITag
    where T7 : struct, ITag
    where T8 : struct, ITag
    where T9 : struct, ITag
    where T10 : struct, ITag
{ }