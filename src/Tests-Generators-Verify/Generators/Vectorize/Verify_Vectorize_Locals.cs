// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Friflo.Vectorization.Generators;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Vectorize;

public static class Verify_Vectorize_Locals
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
    public static async Task  Verify_Query_EmptyBody()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query][OmitHash]
                void EmptyBody(ref Position1 position) { }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_LocalVariable()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position1 : IComponent { public float value; }
            public struct Velocity1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query][OmitHash]
                void LocalVariable(ref Position1 position, Velocity1 velocity) {
                    var localVar = position.value;
                    localVar     = velocity.value;
                    position.value = localVar;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_MixedLocals()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position2      : IComponent { public Vector2 value; }
            public struct FloatComponent : IComponent { public float   value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                private static void MixedLocals(ref Position2 position, in FloatComponent scalarComp, Vector2 vec, float scalar) {
                    Vector2 vec2 = position.value * scalar;
                    float scalar2 = scalarComp.value * scalar;
                    position.value = vec * vec2 * scalar2;
                } 
            }
            """;
        await Verify(code);
    }

}
