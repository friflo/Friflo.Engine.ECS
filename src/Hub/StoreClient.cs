// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

[assembly: CLSCompliant(true)]

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedReadonlyField
namespace Friflo.Engine.Hub;


/// <remarks> commands are implemented by <see cref="StoreCommands"/> </remarks>
[CLSCompliant(true)]
[MessagePrefix("store.")]
public class StoreClient : FlioxClient
{
    // --- containers
    public  readonly    EntitySet <long, DataEntity>   entities;
    
    
    // --- commands
    /// <summary> Run the garbage collector using <c>GC.Collect(generation)</c> </summary>
    public CommandTask<string>              Collect     (int? param)        => send.Command<int?, string>                   (param);
    
    /// <summary> Add the passed <see cref="AddEntities.entities"/> to the <see cref="AddEntities.targetEntity"/> </summary>
    public CommandTask<AddEntitiesResult>   AddEntities (AddEntities param) => send.Command<AddEntities, AddEntitiesResult> (param);
    
    /// <summary> Return the <see cref="DataEntity"/>'s of the passed entity ids including their children. </summary>
    public CommandTask<GetEntitiesResult>   GetEntities (GetEntities param) => send.Command<GetEntities, GetEntitiesResult> (param);
    
    // --- constructor
    public StoreClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}
