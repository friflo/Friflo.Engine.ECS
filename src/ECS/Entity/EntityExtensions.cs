// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;


public static class EntityExtensions
{
    #region non generic component - methods
    /// <summary>
    /// Returns a copy of the entity component as an object.<br/>
    /// The returned <see cref="IComponent"/> is a boxed struct.<br/>
    /// So avoid using this method whenever possible. Use <see cref="Entity.GetComponent{T}"/> instead.
    /// </summary>
    public static  IComponent GetEntityComponent    (this Entity entity, ComponentType componentType) {
        return entity.archetype.heapMap[componentType.structIndex].GetComponentDebug(entity.compIndex);
    }

    public static  bool       RemoveEntityComponent (this Entity entity, ComponentType componentType)
    {
        int archIndex = 0;
        return entity.archetype.entityStore.RemoveComponent(entity.id, ref entity.refArchetype, ref entity.refCompIndex, ref archIndex, componentType.structIndex);
    }
    
    public static  bool       AddEntityComponent    (this Entity entity, ComponentType componentType) {
        return componentType.AddEntityComponent(entity);
    }
    #endregion
    
    #region non generic script - methods
    public static Script GetEntityScript    (this Entity entity, ScriptType scriptType) => EntityUtils.GetScript       (entity, scriptType.type);
    
    public static Script RemoveEntityScript (this Entity entity, ScriptType scriptType) => EntityUtils.RemoveScriptType(entity, scriptType);
    
    public static Script AddNewEntityScript (this Entity entity, ScriptType scriptType) => EntityUtils.AddNewScript    (entity, scriptType);
    
    public static Script AddEntityScript    (this Entity entity, Script script)         => EntityUtils.AddScript       (entity, script);

    #endregion
    
    internal static int ComponentCount (this Entity entity) {
        return entity.archetype.componentCount + entity.Scripts.Length;
    }
}
