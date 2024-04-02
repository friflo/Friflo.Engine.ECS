// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;

// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedField.Global
namespace Friflo.Engine.Hub;

// --------------------------------------- command models ---------------------------------------
public sealed class AddEntities
{
                public  long                targetEntity;
    [Required]  public  List<DataEntity>    entities;
}

public sealed class AddEntitiesResult
{
    /// <summary> Number of entities requested to add. </summary>
                public  int             count;
    /// <summary> Contains errors caused by inconsistent input. E.g. an entity contains itself an an entity.</summary>
    [Required]  public  List<string>    errors;
    /// <summary> Contains new pid's for every entity in <see cref="AddEntities.entities"/> </summary>
    [Required]  public  List<long>      newPids;
}

public sealed class GetEntities
{
    [Required]  public  List<long>  ids;
}

public sealed class GetEntitiesResult
{
    /// <summary> Number of returned entities. </summary>
                public  int         count;
    /// <summary> Contains the requested entities including their children. </summary>
    [Required]  public  JsonValue   entities;
}
