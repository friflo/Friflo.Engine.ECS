using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyNUnit;

namespace Tests.Generators;

public static class VerifyUtils
{
    public static async Task VerifyQuery(string source)
    {
        // 1. Setup (Helper method suggested for readability)
        var compilation = CreateCompilation(source);
        var generator = new AttributeQueryGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 2. Run
        var runResult = driver.RunGenerators(compilation);

        // 3. Verify (NUnit adapter)
        // This creates: MyGeneratorTests.Generator_Snapshot_Test.verified.txt
        await Verifier.Verify(runResult);
    }
 
    
    public static Compilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create("TestProj",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] {
                MetadataReference.CreateFromFile(typeof(QueryAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Numerics.Vector3).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}