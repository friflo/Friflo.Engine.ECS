// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Friflo.Vectorization.Generators;

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
    public          IMethodSymbol                   methodSymbol;
    public          ImmutableArray<AttributeData>   attributes;
    public          ImmutableArray<IParameterSymbol>parameters;
    public          List<IParameterSymbol>          components;
    public          EcsTypes                        ecsTypes;
    public          SourceProductionContext         spc;
    public          SemanticModel                   semanticModel;
    // --- generated output
    public          int                             vectorDimension;    // [3, 4]
    public          int                             laneCount;          // [3, 2]
    public          VectorType[]                    vectorTypes;
    public          string                          hash;
    public          bool                            vectorize;
    public          string                          avxMethod = "";
    public readonly Dictionary<string, ParamType>   paramTypes = new ();
    public readonly StringBuilder                   locals = new ();
    public          int                             constLocalsCount;

    public string AddConst() {
        return $"const{constLocalsCount++}";
    }

    public void ReportDiagnosticSymbol(DiagnosticDescriptor descriptor, ISymbol? locationSymbol, params object?[]? messageArgs)
    {
        var location = locationSymbol?.Locations.FirstOrDefault();
        if (location == null) {
            location = methodSymbol.Locations.FirstOrDefault();
        }
        var diagnostic = Diagnostic.Create(descriptor, location, messageArgs);
        spc.ReportDiagnostic(diagnostic);
    }
    
    public void ReportDiagnosticSyntax(DiagnosticDescriptor descriptor, CSharpSyntaxNode syntaxNode, params object?[]? messageArgs)
    {
        var location = syntaxNode.GetLocation();
        var diagnostic = Diagnostic.Create(descriptor, location, messageArgs);
        spc.ReportDiagnostic(diagnostic);
    }
}

public enum ParamType
{
    None,
    Scalar,
    Vector,
    Matrix4x4
}

public struct VectorType
{
    public IParameterSymbol parameter;
    public string           fullQualifiedName;
    public bool             isComponent;
    public ITypeSymbol      valueType;
    public SpecialType      valueSpecialType;
    public ParamType        paramType;
    public int              dimension;

    public override string ToString() {
        return $"{parameter} : {valueType.Name} ({(paramType == ParamType.Vector ? "vector" : "scalar")})";
    }
}

public struct ConstValue
{
    public string       name;
    public string       value;
    public ParamType    paramType;
}