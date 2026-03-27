using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Friflo.Engine.ECS.Generators;

public struct EcsTypes
{
    public INamedTypeSymbol componentInterface;
    public INamedTypeSymbol entityStruct;
    public INamedTypeSymbol vectorizeAttribute;
    public INamedTypeSymbol omitHashAttribute;
}

public class Query
{
    public  IMethodSymbol                   methodSymbol;
    public  ImmutableArray<AttributeData>   attributes;
    public  List<IParameterSymbol>          components;
    public  EcsTypes                        ecsTypes;
    // --- generated output
    public  string                          hash;
    public  string                          avxMethod = "";
}