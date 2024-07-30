// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Friflo.Engine.ECS.Index;
using Friflo.Engine.ECS.Relations;
using Friflo.Engine.ECS.Utils;
using static Friflo.Engine.ECS.NodeFlags;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Used by the <see cref="EntityStore"/> to store <see cref="Entity"/> components, scripts, tags and child entities
/// internally as an array of nodes.
/// </summary> 
/// <remarks>
/// <see cref="EntityNode"/>'s enable organizing entities in a tree graph structure.<br/>
/// The tree graph is stored in a <see cref="EntityStore"/> starting with a single <see cref="EntityStore.StoreRoot"/> entity.<br/> 
/// <br/>
/// When creating a new entity in an <see cref="EntityStore"/> instantiated with <see cref="PidType.RandomPids"/>
/// it generates a unique random pid assigned to the entity.<br/>
/// Using random pids avoid merge conflicts when multiples users make changes to the same <see cref="EntityStore"/> file / database.<br/>
/// The probability generating the same pid by two different users is:
/// <code>
///     p = 1 - exp(-r^2 / (2 * N))
///     r:  number of new entities added by a user to an existing <see cref="EntityStore"/> (not the number of all entities)
///     N:  number of possible values = int.MaxValue = 2147483647
/// </code>
/// See: https://en.wikipedia.org/wiki/Birthday_problem
/// </remarks>
public struct EntityNode
{
#region public properties
    /// <summary>The <see cref="ECS.Archetype"/> storing the entity.</summary>
    public              Archetype       Archetype   =>  archetype;
    
    /// <summary>Internally used flags assigned to the entity.</summary>
    public              NodeFlags       Flags       =>  archetype != null ? Created : default;
    
    /// <summary>Property only used to see component names encoded by <see cref="isOwner"/>. </summary>
    internal            ComponentTypes  IsOwner     =>  new ComponentTypes{ bitSet = new BitSet { l0 = isOwner } };
    
    /// <summary>Property only used to see component names encoded by <see cref="isLinked"/>. </summary>
    internal            ComponentTypes  IsLinked    =>  new ComponentTypes{ bitSet = new BitSet { l0 = isLinked   } };
                    
    public   override   string          ToString()  => GetString();
    #endregion
    
#region internal fields
    /// <remarks> Is set to null only in <see cref="EntityStore.DeleteNode"/>. </remarks>
    [Browse(Never)] internal    Archetype       archetype;          //  8   can be null. Could use int to relieve GC tracing reference types
    
    [Browse(Never)] internal    int             compIndex;          //  4   index within Archetype.entityIds & StructHeap<>.components
    
    [Browse(Never)] internal    short           revision;           //  2
    
    // /// <summary> Use <see cref="Is"/> or <see cref="IsNot"/> for read access. </summary>
    // [Browse(Never)] internal NodeFlags       flags;              //  1
    
    /// <summary>
    /// Bit mask for all <see cref="EntityRelations"/> and all <see cref="ComponentIndex"/> instances.<br/> 
    /// A bit is set if the entity is an owner of either an entity relation set or an indexed component value.
    /// </summary>
    /// <remarks>
    /// Use <see cref="IsOwner"/> to see <see cref="ComponentTypes"/> by name.<br/>
    /// This masks prevents the insane cost when deleting an entity.<br/>
    /// Otherwise, all <see cref="EntityRelations"/> and <see cref="ComponentIndex"/> instances need to be iterated
    /// to check if the entity is a key in their Dictionary's and perform required cleanup.
    /// </remarks>
    [Browse(Never)] internal    int             isOwner;            //  4
    
    /// <summary>
    /// Bit mask for all <see cref="EntityIndex"/> and all <see cref="EntityRelationLinks{TRelationComponent}"/> instances.<br/> 
    /// A bit is set if the entity is linked by either a <see cref="ILinkComponent"/> or a <see cref="ILinkRelation"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="IsLinked"/> to see <see cref="ComponentTypes"/> by name.<br/>
    /// This masks prevents the insane cost when deleting an entity.<br/>
    /// Otherwise, all <see cref="EntityIndex"/> and <see cref="EntityRelationLinks{TRelationComponent}"/> instances need to be iterated
    /// to check if the entity is a key in their Dictionary's and perform required cleanup.
    /// </remarks>
    [Browse(Never)] internal    int             isLinked;           //  4
    
    /// <remarks> Used to avoid enumeration of <see cref="EntityStore.Intern.signalHandlers"/> </remarks>
                    internal    byte            signalTypeCount;    //  1   number of different signal types attached to the entity.
    
                    internal    HasEventFlags   hasEvent;           //  1   bit is 1 in case an event handler is attached to the entity.
    #endregion
    
#region internal getter
                    // internal readonly   bool        Is      (NodeFlags flag) => (flags & flag) != 0;
                    // internal readonly   bool        IsNot   (NodeFlags flag) => (flags & flag) == 0;
    #endregion
    
#region internal methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAlive(int revision) => archetype != null && this.revision == revision;

    private readonly string GetString()
    {
        var sb = new StringBuilder();
        if (archetype != null) {
            sb.Append("flags: ");
            var startPos = sb.Length;
            if (startPos < sb.Length) {
                sb.Append(" | ");
            }
            sb.Append("Created");
        }
        return sb.ToString();
    }
    #endregion
}

/// <summary>
/// Use to avoid Dictionary lookups for:
/// <see cref="EntityStoreBase.InternBase.entityComponentChanged"/><br/>
/// <see cref="EntityStoreBase.InternBase.entityTagsChanged"/><br/>
/// <see cref="StoreExtension.entityScriptChanged"/><br/>
/// <see cref="StoreExtension.entityChildEntitiesChanged"/><br/>
/// </summary>
[Flags]
internal enum HasEventFlags : byte
{
    /// <summary> Bit is set - <see cref="EntityStoreBase.InternBase.entityComponentChanged"/>.Count > 0<br/> </summary>
    ComponentChanged        = 1,
    /// <summary> Bit is set - <see cref="EntityStoreBase.InternBase.entityTagsChanged"/>.Count > 0<br/> </summary>
    TagsChanged             = 2,
    /// <summary> Bit is set - <see cref="StoreExtension.entityScriptChanged"/>.Count > 0<br/> </summary>
    ScriptChanged           = 4,
    /// <summary> Bit is set - <see cref="StoreExtension.entityChildEntitiesChanged"/>.Count > 0<br/> </summary>
    ChildEntitiesChanged    = 8,
}


