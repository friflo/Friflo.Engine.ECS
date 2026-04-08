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

public static class Verify_Vectorize_Vector3
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
using Friflo.Vectorization;

namespace VerifyVectorize;

public struct Velocity : IComponent { public Vector3 value; }

public partial class MyExample
{
    [Vectorize][Query][OmitHash]
    void MoveExample(ref Position position, in Velocity velocity) {
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
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Velocity : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void MoveExample(ref Position position, in Velocity velocity, float deltaTime) {
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
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Velocity : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position position, Vector3 vector) {
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
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Velocity : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position position, in Velocity velocity, float deltaTime) {
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
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Velocity : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position position, in Velocity velocity, float deltaTime) {
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
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct FloatComponent : IComponent { public float value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void AssignVector(ref Position position, in FloatComponent factor) {
                    position.value = position.value * factor.value;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Lerp()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position3 : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void Lerp(ref Position3 position, Vector3 vec, Vector3 amount) {
                    position.value = Vector3.Lerp(position.value, vec, amount);
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_static()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position3 : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void Access_static(ref Position3 position) {
                    position.value = Vector3.Pi;
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Truncate()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position3 : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void Access_static(ref Position3 position) {
                    position.value = Vector3.Truncate(position.value);
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Cross()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position3 : IComponent { public Vector3 value; }
            public struct Velocity3 : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void Cross(ref Position3 position, Velocity3 velocity) {
                    position.value = Vector3.Cross(position.value, velocity.value);
                }
            }
            """;
        await Verify(code);
    }
    
    [Test]
    public static async Task  Verify_Query_Normalize()
    {
        var code =
            """
            using System.Numerics;
            using Friflo.Engine.ECS;
            using Friflo.Vectorization;
            
            namespace VerifyVectorize;

            public struct Position3 : IComponent { public Vector3 value; }
            public struct Velocity3 : IComponent { public Vector3 value; }

            public partial class MyExample
            {
                [Vectorize][Query]  [OmitHash]
                void Normalize(ref Position3 position, Velocity3 velocity) {
                    position.value = Vector3.Normalize(velocity.value);
                }
            }
            """;
        await Verify(code);
    }
}
