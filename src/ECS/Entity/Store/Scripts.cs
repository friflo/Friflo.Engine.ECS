// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

internal partial struct StoreExtension
{
    // --------------------------------- script methods ---------------------------------
    internal static Script[] GetScripts(Entity entity, int scriptIndex) {
        return entity.store.extension.entityScripts[scriptIndex].scripts;
    }
    
    internal static Script GetScript(Entity entity, Type scriptType, int scriptIndex)
    {
        var scripts = entity.store.extension.entityScripts[scriptIndex].scripts;
        foreach (var script in scripts) {
            if (script.GetType() == scriptType) {
                return script;
            }
        }
        return null;
    }
    
    internal void AppendScript(Entity entity, Script script)
    {
        script.entity = entity;
        if (!entity.store.extension.scriptMap.TryGetValue(entity.Id, out int scriptIndex)) {
            // case: entity has not scripts => add new Scripts entry
            var lastIndex = entityScriptCount++;
            scriptMap.Add(entity.Id, lastIndex);
            if (entityScripts.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                ArrayUtils.Resize(ref entityScripts, newLength);
            }
            entityScripts[lastIndex] = new EntityScripts(entity.Id, new Script[] { script });
        } else {
            // case: entity already has scripts => add script to its scripts
            ref var scripts = ref entityScripts[scriptIndex].scripts;
            var len = scripts.Length;
            ArrayUtils.Resize(ref scripts, len + 1);
            scripts[len] = script;
        }
    }
    
    /// <remarks>
    /// - SEND_EVENT notes -
    /// <br/>
    /// Send event must be last statement <b>AFTER</b> an entity mutation has finished.<br/>
    /// This ensures preserving a valid entity state after an add / remove mutation has finished.<br/>
    /// Reasons: <br/>
    /// - Event handlers expect a valid entity state after add / remove mutation.<br/> 
    /// - When sending an event to the event handlers any of them may throw an exception.
    ///   So this exception will not result in an invalid entity state.<br/>
    /// <br/> 
    /// The entity state refers to:
    /// <list type="bullet">
    ///   <item><see cref="EntityNode.archetype"/></item>
    ///   <item><see cref="EntityNode.compIndex"/></item>
    ///   <item><see cref="RawEntityNode.archIndex"/></item>
    /// </list>
    /// </remarks>
    internal Script AddScript(Entity entity, Script script, ScriptType scriptType)
    {
        Script              currentScript;
        ScriptChangedAction action;
        script.entity = entity;
        if (!scriptMap.TryGetValue(entity.Id, out int scriptIndex))
        {
            // case: entity has not scripts => add new Scripts entry
            action = ScriptChangedAction.Add;
            var lastIndex = entityScriptCount++;
            scriptMap.Add(entity.Id, lastIndex);
            if (entityScripts.Length == lastIndex) {
                var newLength = Math.Max(1, 2 * lastIndex);
                ArrayUtils.Resize(ref entityScripts, newLength);
            }
            entityScripts[lastIndex] = new EntityScripts(entity.Id, new Script [] { script });
            currentScript   = null;
            goto SendEvent;
        }
        // case: entity has already scripts => add / replace script to / in scripts
        action = ScriptChangedAction.Replace;
        ref var entityScript    = ref entityScripts[scriptIndex];
        var scripts             = entityScript.scripts;
        var len                 = scripts.Length;
        for (int n = 0; n < len; n++)
        {
            var current = scripts[n]; 
            if (current.GetType() == scriptType.Type) {
                // case: scripts contains a script of the given scriptType => replace current script
                scripts[n] = script;
                current.entity  = default;
                currentScript   = script;
                goto SendEvent;
            }
        }
        // --- case: scripts does not contain a script of the given scriptType => add script
        ArrayUtils.Resize(ref entityScript.scripts, len + 1);
        entityScript.scripts[len] = script;
        currentScript = null;
    SendEvent:        
        // Send event. See: SEND_EVENT notes
        var added = scriptAdded;
        if (added == null) {
            return currentScript;
        }
        added.Invoke(new ScriptChanged (entity, action, script, currentScript, scriptType));
        return currentScript;
    }
    
    internal Script RemoveScript(Entity entity, ScriptType scriptType, int scriptIndex)
    {
        ref var entityScript    = ref entityScripts[scriptIndex];
        var     scripts         = entityScript.scripts;
        var     len             = scripts.Length;
        for (int n = 0; n < len; n++)
        {
            var script = scripts[n];
            if (script.GetType() != scriptType.Type) {
                continue;
            }
            // case: found script in entity scripts
            script.entity   = default;
            if (len == 1) {
                // case: script is the only one attached to the entity => remove complete scripts entry 
                var lastIndex       = --entityScriptCount;
                if (lastIndex < 1)  throw new InvalidOperationException("invariant: entityScriptCount > 0");
                var lastEntityId    = entityScripts[lastIndex].id;
                // Is the Script not the last in store.entityScripts?
                if (entity.Id != lastEntityId) {
                    // move scriptIndex of last item in store.entityScripts to the index which will be removed
                    entityScripts[scriptIndex]  = entityScripts[lastIndex];
                    scriptMap[lastEntityId]     = scriptIndex;
                }
                entityScripts[lastIndex] = default;                  // clear last Script entry
                scriptMap.Remove(entity.Id);
                goto SendEvent;
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
        SendEvent:
            // Send event. See: SEND_EVENT notes
            var removed = scriptRemoved;
            if (removed == null) {
                return script;
            }
            removed.Invoke(new ScriptChanged (entity, ScriptChangedAction.Remove, null, script, scriptType));
            return script;
        }
        return null;
    }
}
