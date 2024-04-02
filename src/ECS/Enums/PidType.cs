// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Specify the way how an <see cref="EntityStore"/> generates entity <see cref="Entity.Pid"/>'s.
/// </summary>
public enum PidType
{
    /// <summary>
    /// Used to simplify testing as the pid and id of an entity are equal.<br/>
    /// It also increases performance in case ids are consecutively.<br/>
    /// This method is <b>not</b> intended to be used to store entities of an <see cref="EntityStore"/>
    /// in JSON files or in a database.<br/>
    /// </summary>
    /// <remarks>
    /// Disadvantages:<br/>
    /// - Big gaps between ids are wasted memory.<br/>
    /// - When add entities in a database id clashes with entities added by other users are very likely.<br/>
    /// - High probability of merge conflicts caused by id clashes by adding the same entity ids by multiple users. 
    /// </remarks>
    UsePidAsId  = 0,
    /// <summary>
    /// Map random <see cref="Entity.Pid"/>'s to <see cref="Entity.Id"/>'s used within the engine at runtime.<br/>
    /// This method is intended to be used to store entities of an <see cref="EntityStore"/> in JSON files or in a database. 
    /// </summary>
    RandomPids  = 1,
}
