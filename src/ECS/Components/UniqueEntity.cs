// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS;


/// <summary>
/// A <see cref="UniqueEntity"/> is used to assign a unique <c>string</c> to an entity within an <see cref="EntityStore"/>.
/// </summary>
/// <remarks>
/// To find a <see cref="UniqueEntity"/> within an <see cref="EntityStore"/> use <see cref="EntityStoreBase.GetUniqueEntity"/>.<br/>
/// It basically acts as a singleton within an <see cref="EntityStore"/>. 
/// </remarks>
[ComponentKey("unique")]
[ComponentSymbol("UQ",  "255,145,0")]
public struct UniqueEntity : IComponent
{
    /// <summary>Unique string identifier assigned to specific <see cref="Entity"/></summary>
    public          string  uid;  //  8
    
    public override string  ToString() => $"UniqueEntity: '{uid}'";

    public UniqueEntity (string uid) {
        this.uid = uid;
    }
}