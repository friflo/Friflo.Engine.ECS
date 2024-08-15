// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal static class Generic<T1>
    where T1 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index);
}

internal static class Generic<T1,T2>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index);
    
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index);
}

internal static class Generic<T1,T2,T3>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index);
}

internal static class Generic<T1,T2,T3,T4>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index
    );
}

internal static class Generic<T1,T2,T3,T4,T5>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index,
        StructInfo<T5>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index,
        T5: StructInfo<T5>.Index
    );
}

internal static class Generic<T1,T2,T3,T4,T5,T6>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index,
        StructInfo<T5>.Index,
        StructInfo<T6>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index,
        T5: StructInfo<T5>.Index,
        T6: StructInfo<T6>.Index
    );
}

internal static class Generic<T1,T2,T3,T4,T5,T6,T7>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index,
        StructInfo<T5>.Index,
        StructInfo<T6>.Index,
        StructInfo<T7>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index,
        T5: StructInfo<T5>.Index,
        T6: StructInfo<T6>.Index,
        T7: StructInfo<T7>.Index
    );
}

internal static class Generic<T1,T2,T3,T4,T5,T6,T7,T8>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
    where T8 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index,
        StructInfo<T5>.Index,
        StructInfo<T6>.Index,
        StructInfo<T7>.Index,
        StructInfo<T8>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index,
        T5: StructInfo<T5>.Index,
        T6: StructInfo<T6>.Index,
        T7: StructInfo<T7>.Index,
        T8: StructInfo<T8>.Index
    );
}

internal static class Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
    where T8 : struct, IComponent
    where T9 : struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index,
        StructInfo<T5>.Index,
        StructInfo<T6>.Index,
        StructInfo<T7>.Index,
        StructInfo<T8>.Index,
        StructInfo<T9>.Index
    );
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index,
        T5: StructInfo<T5>.Index,
        T6: StructInfo<T6>.Index,
        T7: StructInfo<T7>.Index,
        T8: StructInfo<T8>.Index,
        T9: StructInfo<T9>.Index
    );
}

internal static class Generic<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>
    where T1 : struct, IComponent
    where T2 : struct, IComponent
    where T3 : struct, IComponent
    where T4 : struct, IComponent
    where T5 : struct, IComponent
    where T6 : struct, IComponent
    where T7 : struct, IComponent
    where T8 : struct, IComponent
    where T9 : struct, IComponent
    where T10: struct, IComponent
{
    internal static readonly    ComponentTypes      ComponentTypes  =  ComponentTypes  = new ComponentTypes(
        StructInfo<T1>.Index,
        StructInfo<T2>.Index,
        StructInfo<T3>.Index,
        StructInfo<T4>.Index,
        StructInfo<T5>.Index,
        StructInfo<T6>.Index,
        StructInfo<T7>.Index,
        StructInfo<T8>.Index,
        StructInfo<T9>.Index,
        StructInfo<T10>.Index
        );
    
    internal static readonly    SignatureIndexes    SignatureIndexes= new SignatureIndexes(
        T1: StructInfo<T1>.Index,
        T2: StructInfo<T2>.Index,
        T3: StructInfo<T3>.Index,
        T4: StructInfo<T4>.Index,
        T5: StructInfo<T5>.Index,
        T6: StructInfo<T6>.Index,
        T7: StructInfo<T7>.Index,
        T8: StructInfo<T8>.Index,
        T9: StructInfo<T9>.Index,
        T10:StructInfo<T10>.Index
        );
}

