// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

// ReSharper disable NotAccessedField.Global
[assembly: CLSCompliant(true)]

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnassignedReadonlyField
namespace Friflo.Fliox.Engine.Client;


[CLSCompliant(true)]
[MessagePrefix("editor.")]
public class EntityClient : FlioxClient
{
    // --- containers
    public  readonly    EntitySet <long, DataEntity>   entities;
    
    
    // --- commands
    /// <summary> Run the garbage collector using <c>GC.Collect(generation)</c> </summary>
    public CommandTask<string>              Collect (int? param)        => send.Command<int?, string>                   (param);
    
    /// <summary> Add the passed <see cref="AddEntities.entities"/> to the <see cref="AddEntities.targetEntity"/> </summary>
    public CommandTask<AddEntitiesResult>   Add     (AddEntities param) => send.Command<AddEntities, AddEntitiesResult> (param);
    
    
    // --- constructor
    public EntityClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}

public class AddEntities
{
    public  long                targetEntity;
    public  List<DataEntity>    entities;
}

public class AddEntitiesResult
{
    public  int                 count;
    public  HashSet<long>       missingEntities;
}
