// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public class Systems
{
    private readonly    List<ComponentSystem>   list;
    
    public Systems()
    {
        list    = new List<ComponentSystem>();
    }
    
    public void AddSystem (ComponentSystem system)
    {
        list.Add(system);
    }
    
    public void UpdateSystems ()
    {
        foreach (var system in list)
        {
            system.OnUpdate();
        }
    }
}