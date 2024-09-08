// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is the event for event handlers added to <see cref="EntityStore.OnArchetypeCreate"/>.
/// </summary>
/// <remarks>
/// These events are fired on structural changes moving an existing entity or removing all entities in an existing archetype.
/// </remarks>
public readonly struct  ArchetypeCreate
{
#region fields
    /// <summary>The created <see cref="ECS.Archetype"/>.</summary>
    public  readonly    Archetype Archetype;

    #endregion
    
#region properties
    /// <summary>The <see cref="EntityStore"/> containing the created entity.</summary>
    public              EntityStore Store       => Archetype.entityStore;
    
    public override     string      ToString()  => $"archetype: {Archetype.key.hash} - event > ArchetypeCreate";
    #endregion

#region methods
    internal ArchetypeCreate(Archetype archetype)
    {
        Archetype = archetype;
    }
    #endregion
}