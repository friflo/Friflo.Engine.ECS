// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Friflo.Engine.ECS.Generators;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using VerifyNUnit;

// ReSharper disable InconsistentNaming
namespace Tests.Generators.Query;

public static class Verify_Query
{
    [Test]
    public static async Task  Verify_Query_MovePosition()
    {
        // 1. Your Input Source
        var code =
"""
using Friflo.Engine.ECS;

namespace VerifyQuery;

public partial class MyExample
{
    [Query][OmitHash]
    void MoveExample(ref Position position) {
        position.x = 1;
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
        // This creates: MyGeneratorTests.Generator_Snapshot_Test.verified.txt
        await Verifier.Verify(runResult);
    }
}
