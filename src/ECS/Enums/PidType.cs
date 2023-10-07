// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public enum PidType
{
    /// <summary>
    /// Used to simplify testing as the pid and id of an entity are equal.<br/>
    /// It also increases performance in case ids are consecutively.<br/>
    /// This method is <b>not</b> intended to be used to store entities of a scene in JSON files or in a database.<br/>
    /// </summary>
    /// <remarks>
    /// Disadvantages:<br/>
    /// - Big gaps between ids are wasted memory.<br/>
    /// - When add entities in a database id clashes with entities added by other users are very likely.<br/>
    /// - High probability of merge conflicts caused by id clashes by adding the same entity ids by multiple users. 
    /// </remarks>
    UsePidAsId  = 0,
    /// <summary>
    /// Map random <see cref="EntityNode.Pid"/>'s to internal used <see cref="EntityNode.Id"/>'s.<br/>
    /// This method is intended to be used to store entities of a scene in JSON files or in a database. 
    /// </summary>
    RandomPids  = 1,
}
