using System.Text;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Generators;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Tests.Generators.Query;

// Approach testing with Microsoft.CodeAnalysis.*
// Only kept for reference. Using test plugin "Verify.SourceGenerators" instead
public class Verify_CodeAnalysis
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
}