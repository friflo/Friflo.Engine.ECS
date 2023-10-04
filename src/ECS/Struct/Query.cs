using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public class ArchetypeQuery
{
    private readonly EntityStore store;
    
    internal ArchetypeQuery(EntityStore store) {
        this.store  = store;
    }
}