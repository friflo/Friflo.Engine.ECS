// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

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
    
    public bool IsEntityParameter(IParameterSymbol parameter) {
        return parameter.Name == "entity" && SymbolEqualityComparer.Default.Equals(parameter.Type, entityStruct);
    }

    public bool IsComponent(ITypeSymbol typeSymbol) {
        return typeSymbol.AllInterfaces.Contains(componentInterface);
    }
}

public class Query
{
    public  IMethodSymbol                   methodSymbol;
    public  ImmutableArray<AttributeData>   attributes;
    public  ImmutableArray<IParameterSymbol>parameters;
    public  List<IParameterSymbol>          components;
    public  EcsTypes                        ecsTypes;
    // --- generated output
    public  string                          hash;
    public  bool                            vectorize;
    public  string                          avxMethod = "";
    public  Dictionary<string, ParamType>   paramTypes = new ();
}

public enum ParamType
{
    None = 0,
    Scalar,
}