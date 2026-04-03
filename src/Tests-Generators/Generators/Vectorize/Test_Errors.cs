// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;


public static partial class Test_Errors
{
    // --- Expect:  CSGEN001: Vectorization failed - Expect component type 'MyComponent1' having a field named value at parameter 'comp'
    [Vectorize][Query]  [OmitHash]
    private static void InvalidComponentError(ref MyComponent1 comp, int value) {
        comp.a = value;
    }
    
    [Test]
    public static void InvalidComponent_Call() {
        InvalidComponentErrorQuery(new EntityStore(), 1);
    }
    
    
    // --- Expect:  ECSGEN002: Vectorization failed - operation not supported: Console.WriteLine()
    [Vectorize][Query]  [OmitHash]
    private static void OperationUnsupportedError(ref Position1 comp, float value) {
        Console.WriteLine();
    }
    
    [Test]
    public static void OperationUnsupportedError_Call() {
        OperationUnsupportedErrorQuery(new EntityStore(), 1);
    }
    
    // --- Expect:    ECSGEN003: Vectorization failed - Incompatible parameter types: 'Position2' and 'Position'
    [Vectorize][Query][OmitHash]
    private static void IncompatibleParameterTypesError(ref Position2 pos1, ref Position pos3) {
    }
    
    [Test]
    public static void IncompatibleParameterTypes_Call() {
        IncompatibleParameterTypesErrorQuery(new EntityStore());
    }
    
    // --- Expect:    ECSGEN004: Vectorization failed - Invalid parameter type: 'Entity'
    [Vectorize][Query][OmitHash]
    private static void InvalidParameterTypeError(ref Position2 pos1, Entity entity) {
    }
    
    [Test]
    public static void InvalidParameterTypeError_Call() {
        InvalidParameterTypeErrorQuery(new EntityStore());
    }
    
    // --- Expect:    ECSGEN004: Vectorization failed - Invalid parameter type: 'Entity'
    [Vectorize][Query][OmitHash]
    private static void InvalidStatementError(ref FloatComponent position) {
        if (position.value > 1f) {
        }
    }
    
    [Test]
    public static void InvalidStatementError_Call() {
        InvalidStatementErrorQuery(new EntityStore());
    }

}
