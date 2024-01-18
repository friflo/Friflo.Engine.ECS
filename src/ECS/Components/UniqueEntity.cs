// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS;


public struct UniqueEntity : IComponent
{
    public          string  name;  //  8
    
    public override string  ToString() => $"UniqueEntity: '{name}'";

    public UniqueEntity (string name) {
        this.name = name;
    }
}