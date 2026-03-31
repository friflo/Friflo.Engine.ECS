using Microsoft.CodeAnalysis;

namespace Friflo.Engine.ECS.Generators;

public static class Errors
{
    public static readonly DiagnosticDescriptor InvalidComponentType = new (
        id: "ECSGEN001",                    // Unique ID (e.g., SG001)
        title: "Invalid component type",    // Short title
        messageFormat: "Vectorization failed - Expect component type '{0}' having a field named value at parameter '{1}'", // The message (supports {0}, {1} placeholders)
        category: "Naming",           // Category for filtering
        defaultSeverity: DiagnosticSeverity.Warning, 
        isEnabledByDefault: true
        // description: "Detailed description of why this is an error." // Optional
    );
}