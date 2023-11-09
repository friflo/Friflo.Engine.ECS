// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public partial class GameEntityStore
{
    // --------------------------------- script methods ---------------------------------
    internal Script[] GetScripts(GameEntity entity) {
        return entityScripts[entity.scriptIndex].scripts;
    }
    
    internal Script GetScript(GameEntity entity, Type scriptType)
    {
        var scripts = entityScripts[entity.scriptIndex].scripts;
        foreach (var script in scripts) {
            if (script.GetType() == scriptType) {
                return script;
            }
        }
        return null;
    }
    
    internal void AppendScript(GameEntity entity, Script script)
    {
        script.entity = entity;
        if (entity.scriptIndex == GameEntityUtils.NoScripts) {
            // case: entity has not scripts => add new Scripts entry
            var lastIndex = entity.scriptIndex = entityScriptCount++;
            if (entityScripts.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                Utils.Resize(ref entityScripts, newLength);
            }
            entityScripts[lastIndex] = new EntityScripts(entity.id, new Script[] { script });
        } else {
            // case: entity already has scripts => add script to its scripts
            ref var scripts = ref entityScripts[entity.scriptIndex].scripts;
            var len = scripts.Length;
            Utils.Resize(ref scripts, len + 1);
            scripts[len] = script;
        }
    }
    
    internal Script AddScript(GameEntity entity, Script script, Type scriptType)
    {
        script.entity = entity;
        if (entity.scriptIndex == GameEntityUtils.NoScripts)
        {
            // case: entity has not scripts => add new Scripts entry
            var lastIndex = entity.scriptIndex = entityScriptCount++;
            if (entityScripts.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                Utils.Resize(ref entityScripts, newLength);
            }
            entityScripts[lastIndex] = new EntityScripts(entity.id, new Script [] { script });
            return null;
        }
        // case: entity has already scripts => add / replace script to / in scripts
        ref var entityScript    = ref entityScripts[entity.scriptIndex];
        var scripts             = entityScript.scripts;
        var len                 = scripts.Length;
        for (int n = 0; n < len; n++)
        {
            var current = scripts[n]; 
            if (current.GetType() == scriptType) {
                // case: scripts contains a script of the given scriptType => replace current script
                scripts[n] = script;
                current.entity = null;
                return script;
            }
        }
        // --- case: scripts does not contain a script of the given scriptType => add script
        Utils.Resize(ref entityScript.scripts, len + 1);
        entityScript.scripts[len] = script;
        return null;
    }
    
    internal Script RemoveScript(GameEntity entity, Type scriptType)
    {
        ref var entityScript    = ref entityScripts[entity.scriptIndex];
        var scripts             = entityScript.scripts;
        var len                 = scripts.Length;
        for (int n = 0; n < len; n++)
        {
            var script = scripts[n];
            if (script.GetType() != scriptType) {
                continue;
            }
            // case: found script in entity scripts
            script.entity   = null;
            if (len == 1) {
                // case: script is the only one attached to the entity => remove complete scripts entry 
                var lastIndex       = --entityScriptCount;
                var lastEntityId    = entityScripts[lastIndex].id;
                // Is the Script not the last in store.entityScripts?
                if (entity.id != lastEntityId) {
                    // move scriptIndex of last item in store.entityScripts to the index which will be removed
                    entityScripts[entity.scriptIndex] = entityScripts[lastIndex];
                    nodes[lastEntityId].entity.scriptIndex = entity.scriptIndex;
                }
                entityScripts[lastIndex] = default;               // clear last Script entry
                entity.scriptIndex = GameEntityUtils.NoScripts; // set entity state to: contains no scripts 
                return script;
            }
            // case: entity has two or more scripts. Remove the given one from its scripts
            var newScripts = new Script[len - 1];
            for (int i = 0; i < n; i++) {
                newScripts[i]     = scripts[i];
            }
            for (int i = n + 1; i < len; i++) {
                newScripts[i - 1] = scripts[i];
            }
            entityScript.scripts = newScripts;
            return script;
        }
        return null;
    }
}
