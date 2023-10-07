// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Host;

// ReSharper disable UnassignedReadonlyField
namespace Friflo.Fliox.Engine.Client;

public class SceneClient : FlioxClient
{
    public  readonly    EntitySet <long, DataNode>   entities;
    
    public SceneClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}