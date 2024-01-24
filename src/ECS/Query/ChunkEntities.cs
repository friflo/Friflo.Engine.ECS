// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
public readonly struct ChunkEntities : IEnumerable<Entity>
{
#region public properties
    /// <summary> Return the entity <see cref="Entity.Id"/>'s for the components in a <see cref="Chunk{T}"/>. </summary>
    public              ReadOnlySpan<int>   Ids         => new(Archetype.entityIds, 0, Length);
    public   override   string              ToString()  => GetString();
    #endregion

#region public / internal fields
    /// <summary> The <see cref="Archetype"/> containing the <see cref="Chunk{T}"/> components. </summary>
    public   readonly   Archetype           Archetype;  //  8
    
    /// <summary> The number of entities in <see cref="ChunkEntities"/>. </summary>
    public   readonly   int                 Length;     //  4
    //
    internal readonly   int[]               entityIds;  //  8   - is redundant (archetype.entityIds) but avoid dereferencing for typical access pattern
    #endregion
    
    internal ChunkEntities(Archetype archetype, int componentLen) {
        Archetype   = archetype;
        entityIds   = archetype.entityIds;
        Length      = componentLen;
    }
    
#region public methods
    /// <summary>
    /// Return the entity <see cref="Entity.Id"/> for a <see cref="Chunk{T}"/> component at the given <paramref name="index"/>.
    /// </summary>
    public int this[int index] {
        get {
            if (index < Length) {
                return entityIds[index];
            }
            throw new IndexOutOfRangeException();
        }
    }
    
    /// <summary>
    /// Return the <see cref="Entity"/> for a <see cref="Chunk{T}"/> component at the given <paramref name="index"/>.
    /// </summary>
    public Entity EntityAt(int index) {
        if (index < Length) {
            return new Entity(Archetype.entityStore, entityIds[index]);
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
        return sb.ToString();
    }
    
    internal string GetChunksString() {
        var sb = new StringBuilder();
        sb.Append("Chunks[");
        sb.Append(Length);
        sb.Append("]    Archetype: ");
        Archetype.AppendString(sb);
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
        last        = chunkEntities.Length - 1;
        index       = -1;
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