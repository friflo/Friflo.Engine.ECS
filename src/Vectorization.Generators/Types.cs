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
    public          StringBuilder[]                 lanes;
    public          VectorType[]                    vectorTypes;
    public          string                          hash;
    public          bool                            vectorize;
    public          string                          avxMethod = "";
    public readonly Dictionary<string, Param>       paramTypes = new ();
    public readonly StringBuilder                   locals = new ();
    public readonly StringBuilder                   computeTemp = new ();
    public          int                             computeTempCount;
    public          int                             constLocalsCount;
    public          bool                            requireSoA;
    public          bool                            useSoA;

    public string AddConst() {
        return $"const{constLocalsCount++}";
    }
    public string AddTemp() {
        return $"temp{computeTempCount++}";
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
    
    public void AddParam(string name, bool isComponent, bool isScalar, bool isParam, int dimension)
    {
        paramTypes.Add(name, new Param  { isComponent = isComponent, isScalar = isScalar, isParam = isParam, dimension = dimension });
    }
    
    public string GetVectorName(string name, int i)
    {
        if (!useSoA) {
            if (paramTypes.TryGetValue(name, out var paramSoa)) {
                if (paramSoa.isScalar) {
                    return $"{name}_scalar";
                }
            }
            return $"{name}_{i}";
        }
        if (paramTypes.TryGetValue(name, out var param)) {
            if (param.isComponent) {
                if (param.dimension > 1) {
                    return $"{name}_{i}";
                }
                if (vectorDimension == 2) {
                    return $"{name}_{i / (lanes.Length / 2)}";
                }
                return $"{name}_0";
            }
            if (param.dimension == 1 && param.isScalar && param.isParam) {
                return $"{name}_scalar";
            }
            if (param.dimension == 2 && param.isScalar && param.isParam) { // && param.dimension == 1) {
                return $"{name}_{i % 2}";
            }
            if (param.dimension == 1 && param.isScalar && !param.isParam) { // && param.dimension == 1) {
                return $"{name}_{i % 2}";
            }
        }
        return $"{name}_{i}";
    }
}

public enum ParamType
{
    None,
    Scalar,
    Vector,
    Matrix4x4
}

public struct Param
{
    public bool isComponent;
    public bool isScalar;
    public bool isParam;
    public int  dimension;
}

public struct VectorType
{
    public IParameterSymbol parameter;
    public string           fullQualifiedName;
    public bool             isComponent;
    public bool             isScalar;
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