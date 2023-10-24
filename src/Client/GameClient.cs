// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Fliox.Engine.ECS.Sync;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

[assembly: CLSCompliant(true)]

// ReSharper disable UnassignedReadonlyField
namespace Friflo.Fliox.Engine.Client;

[CLSCompliant(true)]
public class GameClient : FlioxClient
{
    public  readonly    EntitySet <long, DataEntity>   entities;
    
    public GameClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}