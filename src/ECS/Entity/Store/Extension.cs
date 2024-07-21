// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS.Collections;
using Friflo.Engine.ECS.Index;
using Friflo.Engine.ECS.Relations;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Add extended features to an <see cref="EntityStore"/> which are typically not part of an ECS. Like:<br/>
/// - An entity hierarchy with patent / child relationship.<br/>
/// - Permanent ids (pid's) of type long used as an alternative identifier for id of type int.<br/>
/// - Entity <see cref="Script"/>'s to support entity components via OOP.<br/> 
/// </summary>
internal partial struct StoreExtension
{
#region pid storage
    internal                            Random                  randPid;                    //  8   - generate random pid's                       - null if UsePidAsId
    internal readonly                   Dictionary<long, int>   pid2Id;                     //  8   - store the id (value) of a pid (key)         - null if UsePidAsId
    internal readonly                   Dictionary<int, long>   id2Pid;                     //  8   - store the pid (value) of an entity id (key) - null if UsePidAsId
    #endregion
    
#region entity hierarchy
    internal                            int[]                   parentMap;                  //  8
    internal readonly                   IdArrayHeap             childHeap;                  //  8
    // --- events
    internal    Action                <ChildEntitiesChanged>    childEntitiesChanged;       //  8   - fires event on add, insert, remove or delete an Entity
    internal    Dictionary<int, Action<ChildEntitiesChanged>>   entityChildEntitiesChanged; //  8
    #endregion
    
#region entity scripts    
    // --- storage
    /// <summary>Count of entities with one or more <see cref="Script"/>'s</summary>
    [Browse(Never)] internal            int                     entityScriptCount;          //  4   - invariant: > 0  and  <= entityScripts.Length
    /// <summary>Contains implicit all entities with one or more <see cref="Script"/>'s to minimize iteration cost for <see cref="Script.Update"/>.</summary>
    internal                            EntityScripts[]         entityScripts;              //  8   - invariant: entityScripts[0] = 0
    /// <summary>Contains the <see cref="entityScripts"/> index (value) of an entity id (key)</summary>
    internal readonly                   Dictionary<int, int>    scriptMap;                  //  8   - invariant: entityScripts[0] = 0
    
    // --- events
    internal    Action                <ScriptChanged>           scriptAdded;                //  8   - fires event on add script
    internal    Action                <ScriptChanged>           scriptRemoved;              //  8   - fires event on remove script
    internal    Dictionary<int, Action<ScriptChanged>>          entityScriptChanged;        //  8   - entity event handlers for add/remove script
    #endregion
    
#region component indices
    internal                            ComponentIndex[]        indexMap;                   //  8   - map & its component indexes created on demand
    #endregion
    
#region entity relations
    internal                            EntityRelations[]       relationsMap;               //  8   - map & its EntityRelations created on demand
    #endregion
    
    internal StoreExtension(PidType pidType)
    {
        parentMap   = Array.Empty<int>();
        childHeap   = new IdArrayHeap();
        if (pidType == PidType.RandomPids) {
            randPid  = new Random();
            pid2Id   = new Dictionary<long, int>();
            id2Pid   = new Dictionary<int, long>();
        }
        scriptMap           = new Dictionary<int, int>();
        entityScripts       = new EntityScripts[1]; // invariant: entityScripts[0] = 0
        entityScriptCount   = 1;
    }
    
    internal void RemoveEntity(int id) {
        if (id2Pid != null) {
            var pid = id2Pid[id];
            id2Pid.Remove(id);
            pid2Id.Remove(pid);
        }
        if (scriptMap.Remove(id)) {
            RemoveEntityScript(id);
        }
    }
    
    internal void GenerateRandomPidForId(int id)
    {
        while(true) {
            // generate random int to have numbers with small length e.g. 2147483647 (max int)
            // could also generate long which requires more memory when persisting entities
            long pid = randPid.Next();
            if (pid2Id.TryAdd(pid, id)) {
                id2Pid.Add(id, pid);
                return;
            }
        }
    }
    
    private void RemoveEntityScript(int id)
    {
        var scripts = entityScripts;
        int len     = entityScriptCount - 1;
        for (int n = 1; n <= len; n++) {
            if (scripts[n].id != id) continue;
            for (; n < len; n++) {
                scripts[n] = scripts[n + 1];
            }
            entityScriptCount   = len;
            scripts[len]        = default;
            break;
        }
    } 
}
