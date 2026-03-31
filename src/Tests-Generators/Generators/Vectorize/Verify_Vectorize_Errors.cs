// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Friflo.Engine.ECS.Generators;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Vectorize;

public static class Verify_Vectorize_Errors
{
    private static async Task Verify(string code)
    {
        // 1. Setup (Helper method suggested for readability)
        var compilation = VerifyUtils.CreateCompilation(code);
        var generator = new AttributeQueryGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 2. Run
        var runResult = driver.RunGenerators(compilation);

        // 3. Verify (NUnit adapter)
        await Verifier.Verify(runResult).IgnoreGeneratedResult(VerifyUtils.IgnoreStaticSource);
    }
    
    [Test]
    public static async Task  Verify_InvalidComponent()
    {
        var code =
"""
using System.Numerics;
using Friflo.Engine.ECS;

namespace VerifyVectorize;

public struct MyComponent1 : IComponent { public float a; }

public partial class MyExample
{
    [Vectorize][Query][OmitHash]
    void InvalidComponent(ref MyComponent1 component, float value) {
        component.a = value;
    }
}
""";
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_OperationUnsupported()
    {
        var code =
        """
        using System.Numerics;
        using Friflo.Engine.ECS;

        namespace VerifyVectorize;

        public struct Position1 : IComponent { public float value; }

        public partial class MyExample
        {
            [Vectorize][Query][OmitHash]
            private static void OperationUnsupportedError(ref Position1 comp, float value) {
                comp.value = MathF.Sin(value);
            }
        }
        """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_IncompatibleParameterTypes()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position2 : IComponent { public Vector2 value; }
            public struct Position3 : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query][OmitHash]
                private static void IncompatibleParameterTypesError(ref Position2 pos1, ref Position3 pos3) {
                }
            }
            """;
        await Verify(code);
    }

}
