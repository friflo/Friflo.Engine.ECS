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
        return entityScripts[entity.scriptIndex].classes;
    }
    
    internal Script GetScript(GameEntity entity, Type scriptType)
    {
        var classes = entityScripts[entity.scriptIndex].classes;
        foreach (var script in classes) {
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
            ref var classes = ref entityScripts[entity.scriptIndex].classes;
            var len = classes.Length;
            Utils.Resize(ref classes, len + 1);
            classes[len] = script;
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
        var classes             = entityScript.classes;
        var len                 = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var current = classes[n]; 
            if (current.GetType() == scriptType) {
                // case: scripts contains a script of the given scriptType => replace current script
                classes[n] = script;
                current.entity = null;
                return script;
            }
        }
        // --- case: scripts does not contain a script of the given scriptType => add script
        Utils.Resize(ref entityScript.classes, len + 1);
        entityScript.classes[len] = script;
        return null;
    }
    
    internal Script RemoveScript(GameEntity entity, Type scriptType)
    {
        ref var entityScript    = ref entityScripts[entity.scriptIndex];
        var classes             = entityScript.classes;
        var len                 = classes.Length;
        for (int n = 0; n < len; n++)
        {
            var script = classes[n];
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
            var scripts = new Script[len - 1];
            for (int i = 0; i < n; i++) {
                scripts[i]     = classes[i];
            }
            for (int i = n + 1; i < len; i++) {
                scripts[i - 1] = classes[i];
            }
            entityScript.classes = scripts;
            return script;
        }
        return null;
    }
}
