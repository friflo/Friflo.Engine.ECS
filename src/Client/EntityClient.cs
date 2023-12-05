// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

[assembly: CLSCompliant(true)]

// ReSharper disable UnassignedReadonlyField
namespace Friflo.Fliox.Engine.Client;


[CLSCompliant(true)]
[MessagePrefix("editor.")]
public class EntityClient : FlioxClient
{
    public  readonly    EntitySet <long, DataEntity>   entities;
    
    public CommandTask<string>      Collect (int? param)    => send.Command<int?, string>    (param);
    
    public EntityClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}