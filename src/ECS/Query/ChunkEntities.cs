// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Provide the entity <see cref="Entity.Id"/>'s for <see cref="Chunk{T}"/> components using <see cref="Ids"/> or <see cref="this[int]"/>.<br/>
/// </summary>
/// <remarks>
/// Its <see cref="Length"/> is equal to the <see cref="Chunk{T}"/>.<see cref="Chunk{T}.Length"/>.<br/>
/// <br/>
/// It implements <see cref="IEnumerable{T}"/> only to provide comprehensive information of <see cref="Entity"/>'s in a debugger.<br/>
/// Its unlikely to enumerate <see cref="ChunkEntities"/> in an application.<br/>
/// The recommended methods used by an application are <see cref="Ids"/>, <see cref="this[int]"/> or <see cref="EntityAt"/>.  
/// </remarks>
[DebuggerTypeProxy(typeof(ChunkEntitiesDebugView))]
public readonly struct ChunkEntities : IEnumerable<Entity>
{
#region public properties
    /// <summary> Return the entity <see cref="Entity.Id"/>'s for the components in a <see cref="Chunk{T}"/>. </summary>
    public              ReadOnlySpan<int>   Ids             => new(Archetype.entityIds, Start, Length);
    
    public   override   string              ToString()      => GetString();
    #endregion

#region public / internal fields
    /// <summary> The <see cref="Archetype"/> containing the <see cref="Chunk{T}"/> components. </summary>
    public   readonly   Archetype           Archetype;  //  8
    
    // ReSharper disable once NotAccessedField.Local
    public   readonly   int                 Start;      //  4
    
    /// <summary> The number of entities in <see cref="ChunkEntities"/>. </summary>
    public   readonly   int                 Length;     //  4
    
    /// <summary> The execution type used to provide the chunk entities. </summary>
    public   readonly   JobExecution        Execution;  //  1
    
    /// <summary>
    /// if    0 - The entities are provided from the main (caller) thread using <c>foreach(...)</c> loop,
    /// <see cref="QueryJob.Run"/> or <see cref="QueryJob.RunParallel"/>.<br/>
    /// if >= 1 - The entities are provided from a worker thread using <see cref="QueryJob.RunParallel"/>.
    /// </summary>
    public   readonly   byte                TaskIndex;  //  1
    //
    internal readonly   int[]               entityIds;  //  8   - is redundant (archetype.entityIds) but avoid dereferencing for typical access pattern
    #endregion
    
    internal ChunkEntities(Archetype archetype, int componentLen, int start) {
        Archetype   = archetype;
        entityIds   = archetype.entityIds;
        Length      = componentLen;
        Start       = start;
    }
    
    internal ChunkEntities(in ChunkEntities entities, int start, int componentLen, int taskIndex) {
        Archetype   = entities.Archetype;
        Execution   = JobExecution.Parallel;
        TaskIndex   = (byte)taskIndex;
        entityIds   = entities.entityIds;
        Start       = start;
        Length      = componentLen;
    }
    
    internal ChunkEntities(in ChunkEntities entities, int taskIndex) {
        Archetype   = entities.Archetype;
        Execution   = JobExecution.Parallel;
        TaskIndex   = (byte)taskIndex;
    }
    
#region public methods
    /// <summary>
    /// Return the entity <see cref="Entity.Id"/> for a <see cref="Chunk{T}"/> component at the given <paramref name="index"/>.
    /// </summary>
    public int this[int index] {
        get {
            if (index < Length) {
                return entityIds[Start + index];
            }
            throw new IndexOutOfRangeException();
        }
    }
    
    /// <summary>
    /// Return the <see cref="Entity"/> for a <see cref="Chunk{T}"/> component at the given <paramref name="index"/>.
    /// </summary>
    public Entity EntityAt(int index) {
        if (index < Length) {
            return new Entity(Archetype.entityStore, entityIds[Start + index]);
        }
        throw new IndexOutOfRangeException();
    }
    
    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new ChunkEntitiesEnumerator(this);
    
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new ChunkEntitiesEnumerator(this);
    
    // --- new
    public ChunkEntitiesEnumerator          GetEnumerator() => new ChunkEntitiesEnumerator(this);
    
    private string GetString() {
        var sb = new StringBuilder();
        sb.Append("Entity[");
        sb.Append(Length);
        sb.Append("]    Archetype: ");
        Archetype.AppendString(sb);
        sb.Append("  entities: ");
        sb.Append(Archetype.entityCount);
        return sb.ToString();
    }
    
    internal string GetChunksString() {
        var sb = new StringBuilder();
        sb.Append("Chunks[");
        sb.Append(Length);
        sb.Append("]    Archetype: ");
        Archetype.AppendString(sb);
        sb.Append("  entities: ");
        sb.Append(Archetype.entityCount);
        return sb.ToString();
    }

    #endregion
}

/// <summary>
/// Used to enumerate the <see cref="Entity"/>'s of <see cref="ChunkEntities"/>.
/// </summary>
public struct ChunkEntitiesEnumerator : IEnumerator<Entity>
{
    private readonly    int[]           entityIds;      //  8
    private readonly    EntityStore     store;          //  8
    private readonly    int             last;           //  4
    private             int             index;          //  4
    
    internal ChunkEntitiesEnumerator(in ChunkEntities chunkEntities) {
        entityIds   = chunkEntities.entityIds;
        store       = chunkEntities.Archetype.entityStore;
        index       = chunkEntities.Start - 1;
        last        = chunkEntities.Length + index;
    }
    
    // --- IEnumerator<>
    /// <summary> The current <see cref="Entity"/> of the enumerator. </summary>
    public readonly Entity Current   => new Entity(store, entityIds[index]);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < last) {
            index++;
            return true;
        }
        return false;  
    }

    public void Reset() {
        index = -1;
    }
    
    object IEnumerator.Current => Current;

    // --- IDisposable
    public void Dispose() { }
}

internal class ChunkEntitiesDebugView
{
    [Browse(RootHidden)]
    public              Entity[]        Entities => GetEntities();

    [Browse(Never)]
    private readonly    ChunkEntities   chunkEntities;
        
    internal ChunkEntitiesDebugView(ChunkEntities chunkEntities)
    {
        this.chunkEntities = chunkEntities;
    }
    
    private Entity[] GetEntities()
    {
        var entities = new Entity[chunkEntities.Length];
        int n = 0; 
        foreach (var entity in chunkEntities) {
            entities[n++] = entity;
        }
        return entities;
    }
}