using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using VerifyNUnit;


namespace Tests.Generators.Query;

[TestFixture]
public static class Verify_Query
{
    // [Test]
    public static async Task  Verify_Query_MovePosition_xxx()
    {
        var code =
"""
using Friflo.Engine.ECS;

namespace Verify;

public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position) {
        position.x = 1;
    }
}
""";
        
var expected =
"""
using Friflo.Engine.ECS;

namespace Verify;

public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position) {
        position.x = 1;
    }
}
""";
        var context = new CSharpSourceGeneratorTest<AttributeQueryGenerator, DefaultVerifier>();
        context.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        context.TestState.AdditionalReferences.Add(typeof(QueryAttribute).Assembly);
        context.TestState.GeneratedSources.Add((typeof(AttributeQueryGenerator), "Verify.MyExample/MoveExample_19EA.g.cs", SourceText.From(expected, Encoding.UTF8)));
        context.TestCode = code;
        await context.RunAsync();

    }
    
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
    [Query]
    void MoveExample(ref Position position) {
        position.x = 1;
    }
}
""";
        // 2. Setup (Helper method suggested for readability)
        var compilation = CreateCompilation(code);
        var generator = new AttributeQueryGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 3. Run
        var runResult = driver.RunGenerators(compilation);

        // 4. Verify (NUnit adapter)
        // This creates: MyGeneratorTests.Generator_Snapshot_Test.verified.txt
        await Verifier.Verify(runResult);

    }
    
    private static Compilation CreateCompilation(string source) =>
        CSharpCompilation.Create("TestProj",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { MetadataReference.CreateFromFile(typeof(QueryAttribute).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}