// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS;


public struct UniqueName : IComponent
{
    public          string  value;  //  8
    
    public override string  ToString() => $"UniqueName: '{value}'";

    public UniqueName (string value) {
        this.value = value;
    }
}