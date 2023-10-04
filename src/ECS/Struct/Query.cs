using System;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public class ArchetypeQuery
{
#region public properties
    public  ReadOnlySpan<Archetype>     Archetypes => new (archetypes);
    #endregion
    
#region private fields
    private readonly    EntityStore     store;
    private             Archetype[]     archetypes;
    #endregion
    
    internal ArchetypeQuery(EntityStore store) {
        this.store  = store;
        archetypes  = Array.Empty<Archetype>();
    }
}