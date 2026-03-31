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

public static class Verify_Vectorize_Vector2
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
    public static async Task  Verify_Query_MovePosition()
    {
        var code =
"""
using System.Numerics;
using Friflo.Engine.ECS;

namespace VerifyVectorize;

public struct Position2 : IComponent { public Vector2 value; }
public struct Velocity2 : IComponent { public Vector2 value; }

public partial class MyExample
{
    [Vectorize][Query][OmitHash]
    void MoveExample(ref Position2 position, in Velocity2 velocity) {
        position.value *= velocity.value;
    }
}
""";
        await Verify(code);
    }

    [Test]
    public static async Task  Verify_Query_MovePosition_deltaTime()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position2 : IComponent { public Vector2 value; }
            public struct Velocity2 : IComponent { public Vector2 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void MoveExample(ref Position2 position, in Velocity2 velocity, float deltaTime) {
                    position.value *= velocity.value * deltaTime;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_AssignVector()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position2 : IComponent { public Vector2 value; }
            public struct Velocity2 : IComponent { public Vector2 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position2 position, Vector2 vector) {
                    position.value = vector;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_MultiplyAdd_Assignment()
    {

        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Velocity2 : IComponent { public Vector2 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position2 position, in Velocity2 velocity, float deltaTime) {
                    position.value += velocity.value * deltaTime;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_MultiplyAdd()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position2 : IComponent { public Vector2 value; }
            public struct Velocity2 : IComponent { public Vector2 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position2 position, in Velocity2 velocity, float deltaTime) {
                    position.value = velocity.value * deltaTime + position.value;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_scalar_component()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position2 : IComponent { public Vector2 value; }
            public struct FloatComponent : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position2 position, in FloatComponent factor) {
                    position.value = position.value * factor.value;
                }
            }
            """;
        await Verify(code);
    }
}
