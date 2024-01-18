// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS;


/// <summary>
/// A <see cref="UniqueEntity"/> component is intended to be added to exactly on entity within an <see cref="EntityStore"/>.<br/>
/// This entity can be retrieved with <see cref="EntityStore.GetUniqueEntity"/>.<br/>
/// It basically acts as a singleton within an <see cref="EntityStore"/>. 
/// </summary>
[ComponentKey("unique")]
public struct UniqueEntity : IComponent
{
    public          string  name;  //  8
    
    public override string  ToString() => $"UniqueEntity: '{name}'";

    public UniqueEntity (string name) {
        this.name = name;
    }
}