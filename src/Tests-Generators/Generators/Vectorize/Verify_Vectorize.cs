using System.Threading.Tasks;
using Friflo.Engine.ECS.Generators;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using VerifyNUnit;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Vectorize;

public static class Verify_Vectorize
{
    [Test]
    public static async Task  Verify_Query_MovePosition()
    {
        // 1. Your Input Source
        var code =
"""
using Friflo.Engine.ECS;

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
        // 2. Setup (Helper method suggested for readability)
        var compilation = VerifyUtils.CreateCompilation(code);
        var generator = new AttributeQueryGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 3. Run
        var runResult = driver.RunGenerators(compilation);

        // 4. Verify (NUnit adapter)
        await Verifier.Verify(runResult);
    }
    
    [Test]
    public static async Task  Verify_Query_MovePosition_deltaTime()
    {
        // 1. Your Input Source
        var code =
            """
            using Friflo.Engine.ECS;

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
        // 2. Setup (Helper method suggested for readability)
        var compilation = VerifyUtils.CreateCompilation(code);
        var generator = new AttributeQueryGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 3. Run
        var runResult = driver.RunGenerators(compilation);

        // 4. Verify (NUnit adapter)
        await Verifier.Verify(runResult);
    }
    
    [Test]
    public static async Task  Verify_Query_AssignVector()
    {
        // 1. Your Input Source
        var code =
            """
            using Friflo.Engine.ECS;

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
        // 2. Setup (Helper method suggested for readability)
        var compilation = VerifyUtils.CreateCompilation(code);
        var generator = new AttributeQueryGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 3. Run
        var runResult = driver.RunGenerators(compilation);

        // 4. Verify (NUnit adapter)
        await Verifier.Verify(runResult);
    }
}
