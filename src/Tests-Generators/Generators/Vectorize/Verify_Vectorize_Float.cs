// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Friflo.Vectorize.Generators;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Vectorize;

public static class Verify_Vectorize_Float
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

public struct Position1 : IComponent { public float value; }
public struct Velocity1 : IComponent { public float value; }

public partial class MyExample
{
    [Vectorize][Query][OmitHash]
    void MoveExample(ref Position1 position, in Velocity1 velocity) {
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

            public struct Position1 : IComponent { public float value; }
            public struct Velocity1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void MoveExample(ref Position1 position, in Velocity1 velocity, float deltaTime) {
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

            public struct Position1 : IComponent { public float value; }
            public struct Velocity1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position1 position, float vector) {
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

            public struct Position1 : IComponent { public float value; }
            public struct Velocity1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position1 position, in Velocity1 velocity, float deltaTime) {
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

            public struct Position1 : IComponent { public float value; }
            public struct Velocity1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position1 position, in Velocity1 velocity, float deltaTime) {
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

            public struct Position1 : IComponent { public float value; }
            public struct FloatComponent : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position1 position, in FloatComponent factor) {
                    position.value = position.value * factor.value;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Min()
    {
        var code =
            """
            using System;
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position1 position, float max) {
                    position.value = MathF.Min(position.value, max);
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Clamp()
    {
        var code =
            """
            using System;
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position1 position, float min, float max) {
                    position.value = Math.Clamp(position.value, min, max);
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Const()
    {
        var code =
            """
            using System;
            using System.Numerics;
            using Friflo.Engine.ECS;

            namespace VerifyVectorize;

            public struct Position1 : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignConst(ref Position1 position) {
                    position.value = 1;
                }
            }
            """;
        await Verify(code);
    }
}
