// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public sealed class Systems
{
    private readonly    List<ComponentSystem>   list;

    public  override    string                  ToString() => $"Count: {list.Count}";

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