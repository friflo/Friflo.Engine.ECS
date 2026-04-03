using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Vectorization;
using Friflo.Vectorization.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyNUnit;
using VerifyTests;

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
        await Verifier.Verify(runResult).IgnoreGeneratedResult(VerifyUtils.IgnoreStaticSource);
    }
 
    
    public static Compilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create("TestProj",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] {
                MetadataReference.CreateFromFile(typeof(VectorizeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Numerics.Vector3).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static bool IgnoreStaticSource(GeneratedSourceResult result)
    {
        if (result.HintName.Equals("Friflo.Engine.ECS.Intrinsics/AvxUtils.g.cs")) {
            return true;
        }
        return false;
    } 
}