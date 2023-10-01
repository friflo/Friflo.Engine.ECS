using System;
using static Fliox.Engine.ECS.TreeMembership;
using static Fliox.Engine.ECS.StoreOwnership;

// ReSharper disable InconsistentNaming
namespace Fliox.Engine.ECS;

/// <summary>Describe the ownership state of a <see cref="GameEntity"/></summary>
public enum StoreOwnership
{
    /// <summary>The entity is owned by an <see cref="EntityStore"/></summary>
    /// <remarks>
    /// Entities created with <see cref="EntityStore.CreateEntity()"/> are automatically <see cref="attached"/> to its <see cref="EntityStore"/><br/>
    /// </remarks>
    attached = 1,
    /// <summary>The entity is not owned by an <see cref="EntityStore"/></summary>
    /// <remarks>
    /// When calling <see cref="GameEntity.DeleteEntity"/> its state changes to <see cref="detached"/>.<br/>
    /// </remarks>
    detached = 0,
}

/// <summary>Describe the membership of a <see cref="GameEntity"/> to the tree of an <see cref="EntityStore"/></summary>
/// <remarks>Requirement: The entity must be <see cref="attached"/> to an <see cref="EntityStore"/></remarks>
public enum TreeMembership
{
    /// <summary>The entity is member of the <see cref="EntityStore"/> tree</summary>
    treeNode = 1,
    /// <summary>The entity is not member of the <see cref="EntityStore"/> tree</summary>
    floating = 0,
}

[Flags]
public enum NodeFlags : byte
{
    NullNode        = 0b_0000_0000,
    Created         = 0b_0000_0001,
    /// <summary>
    /// If set node is a <see cref="treeNode"/>. Otherwise <see cref="floating"/>
    /// </summary>
    TreeNode        = 0b_0000_0010,
    // - prefab flags
    PrefabLink  = 0b_0001_0000, // link to prefab location
    OpMask      = 0b_0000_1100,
    OpKeep      = 0b_0000_0100, // keep components of prefab entity as they are
    OpModify    = 0b_0000_1000, // modify components of prefab entity
    OpRemove    = 0b_0000_1100, // remove prefab entity
}

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
    UsePidAsId,
    /// <summary>
    /// Map random <see cref="EntityNode.Pid"/>'s to internal used <see cref="EntityNode.Id"/>'s.<br/>
    /// This method is intended to be used to store entities of a scene in JSON files or in a database. 
    /// </summary>
    RandomPids
}