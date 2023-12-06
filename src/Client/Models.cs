// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Serialize;

// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedField.Global
namespace Friflo.Fliox.Engine.Client;

// --------------------------------------- command models ---------------------------------------
public class AddEntities
{
    public  long                targetEntity;
    public  List<DataEntity>    entities;
}

public class AddEntitiesResult
{
    /// <summary> Number of entities requested to add. </summary>
    public  int                 count;
    /// <summary> Contains new pid's for every entity in <see cref="AddEntities.entities"/> </summary>
    public  List<long?>         added;
    /// <summary> Contains pid's used in <see cref="DataEntity.children"/> but missing in <see cref="AddEntities.entities"/> </summary>
    public  HashSet<long>       missingEntities;
    /// <summary> Contains pid's failed to add because of inconsistent input. E.g. an entity contains itself an an entity.</summary>
    public  HashSet<long>       addErrors;
}
