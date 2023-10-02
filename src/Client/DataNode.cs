// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Host;

// ReSharper disable ClassNeverInstantiated.Global
namespace Friflo.Fliox.Engine.Client;

/// <summary>
/// A <see cref="DataNode"/> is used to serialize entities.<br/>
/// The <see cref="EntityStoreClient"/> provide methods to create, read, update, delete and query entities.
/// </summary>
/// <remarks>
/// When reading / writing <see cref="DataNode"/>'s in the editor a special <see cref="MemoryDatabase"/> implementation is required.<br/>
/// This implementation preserves the order of entities stored in a JSON file.<br/>
/// Therefor it stores the order of each entity when loaded and apply this order when writing them back to the JSON file.<br/> 
/// <br/>
/// Having a stable order is required avoid merge conflicts.<br/>
/// Otherwise the entity order in a JSON file would be arbitrary.<br/>
/// Even small changes will show a massive diff in version control. 
/// </remarks>
public sealed class DataNode
{
    /// <summary>permanent id used to identify entities in a database</summary>
    [Serialize        ("id")] 
    public  int         pid;
    
    /// <remarks>
    /// Use a list of child ids instead of a single field <c>parentId</c> to enable child order.<br/>
    /// <br/>
    /// An alternative order implementation - using firstChild, nextSibling - is error prone if referenced nodes are missing.<br/>
    /// For now the child order is required to enable a memorable order in the editor and to avoid merge conflicts. 
    /// </remarks>
    public  List<int>   children;       // can be null
    
    /// <summary>
    /// ComponentWriter: create a <see cref="JsonValue"/> from all class / struct components of an entity for serialization.<br/>
    /// ComponentReader: create all class / struct components for an entity from <see cref="JsonValue"/>
    ///                  when calling <see cref="EntityStore.CreateFromDataNode"/><br/>
    /// <br/>
    /// Each key in <see cref="components"/> defines the type of a class / struct component. Its value is the component value.
    /// </summary>
    public  JsonValue   components;     // can be null
    
    /// <summary>Reference to the `PreFab` the entity is based on</summary>
    public  string      preFab;         // can be null
    
    /// <summary>
    /// Modify the referenced node of a`preFab`.<br/> with <see cref="components"/> != null<br/>
    /// Remove the referenced node if <see cref="components"/> == null
    /// </summary>
    public  string      modify;         // can be null

    
    public  override string ToString() => $"id: {pid}";
}
