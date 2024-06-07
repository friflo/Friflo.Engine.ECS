// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public interface ICommandBuffer
{
#region general
    public void Clear();
    public void Playback();
    #endregion
    
#region component
    public void AddComponent  <T>(int entityId)                     where T : struct, IComponent;
    public void AddComponent   <T>(int entityId, in T component)    where T : struct, IComponent;
    public void SetComponent   <T>(int entityId, in T component)    where T : struct, IComponent;
    public void RemoveComponent<T>(int entityId)                    where T : struct, IComponent;
    #endregion
    
#region tag
    public void AddTag   <T>(int entityId)                  where T : struct, ITag;
    public void AddTags     (int entityId, in Tags tags);
    public void RemoveTag<T>(int entityId)                  where T : struct, ITag;
    public void RemoveTags  (int entityId, in Tags tags);
    #endregion

#region script
    public void AddScript   <T>(int entityId, T script)     where T : Script, new();
    public void RemoveScript<T>(int entityId)               where T : Script, new();
    #endregion
    
#region child entity
    public void AddChild    (int parentId, int childId);
    public void RemoveChild (int parentId, int childId);
    #endregion

#region entity
    public int  CreateEntity();
    public void DeleteEntity(int entityId);
    #endregion
}

