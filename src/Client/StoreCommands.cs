// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox;
using Friflo.Json.Fliox.Hub.Host;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
namespace Friflo.Fliox.Engine.Client;

/// <summary>
/// Implementation of <see cref="EntityClient"/> commands.
/// </summary>
public class StoreCommands : IServiceCommands
{
    /// <remarks> Must be accessed only from main thread. </remarks>
    private readonly EntityStore    store;
        
    public StoreCommands(EntityStore store) {
        this.store = store;
    }
        
    [CommandHandler("store.Collect")]
    private static Result<string> Collect(Param<int?> param, MessageContext context)
    {
        if (!param.GetValidate(out var nullableGeneration, out var error)) {
            return Result.ValidationError(error);
        }
        int generation = nullableGeneration ?? 1;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        GC.Collect(generation);
        
        double elapsedTicks    = stopwatch.ElapsedTicks;
        var duration        = 1000 * elapsedTicks / Stopwatch.Frequency;
        var msg = $"GC.Collect({generation}) - duration: {duration} ms";
        Console.WriteLine(msg);

        return msg;
    }
    
    [CommandHandler("store.AddEntities")]
    private async Task<Result<AddEntitiesResult>> AddEntities(Param<AddEntities> param, MessageContext context)
    {
        if (!param.GetValidate(out var addEntities, out var error)) {
            return Result.ValidationError(error);
        }
        if (addEntities == null) {
            return Result.Error("missing param");
        }
        return await StoreUtils.InvokeAsync(() => Task.FromResult(AddEntitiesInternal(addEntities)));
    }
    
    private Result<AddEntitiesResult> AddEntitiesInternal (AddEntities addEntities)
    {
        if (!store.TryGetEntityByPid(addEntities.targetEntity, out var targetEntity)) {
            return Result.Error($"targetEntity not found. was: {addEntities.targetEntity}");
        }
        var entities    = addEntities.entities;
        var result      = ECSUtils.AddDataEntitiesToEntity(targetEntity, entities);
        
        var added       = new List<long?>(entities.Count);
        var missingPids = result.missingPids;
        foreach (var entity in entities) {
            if (result.addedEntities.Contains(entity.pid)) {
                added.Add(entity.pid);
                continue;
            }
            added.Add(null);
        }
        return new AddEntitiesResult {
            count           = addEntities.entities.Count,
            missingEntities = missingPids,
            addErrors       = result.addErrors,
            added           = added
        };
    }
    
    [CommandHandler("store.GetEntities")]
    private async Task<Result<GetEntitiesResult>> GetEntities(Param<GetEntities> param, MessageContext context)
    {
        if (!param.GetValidate(out var getEntities, out var error)) {
            return Result.ValidationError(error);
        }
        if (getEntities == null) {
            return Result.Error("missing param");
        }
        return await StoreUtils.InvokeAsync(() => Task.FromResult(GetEntitiesInternal(getEntities)));
    }
    
    private Result<GetEntitiesResult> GetEntitiesInternal(GetEntities getEntities)
    {
        var ids = getEntities.ids;
        var entities = new List<Entity>(ids.Count);
        foreach (var pid in ids) {
            if (!store.TryGetEntityByPid(pid, out var entity)) {
                return Result.Error($"pid not found. was: {pid}");
            }
            entities.Add(entity);
        }
        var result = ECSUtils.EntitiesToJsonArray(entities);
        return new GetEntitiesResult { count = result.count, entities = result.entities };
    }
}

