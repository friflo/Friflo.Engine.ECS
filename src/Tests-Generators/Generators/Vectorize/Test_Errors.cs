// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS;
using Tests.ECS;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators.Vectorize;


public static partial class Test_Errors
{
    [Vectorize][Query]  [OmitHash]
    private static void InvalidComponentError(ref MyComponent1 comp, int value) {
        comp.a = value;
    }
    private static void InvalidComponentCall() => InvalidComponentErrorQuery(null, 1);
    
    
}
