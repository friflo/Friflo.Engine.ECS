using Friflo.Engine.ECS.Relations;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Storage for all entity relations of the specified <typeparamref name="TRelation"/> type.<br/>
/// An instance is returned via <see cref="RelationExtensions.EntityRelations{TRelation}"/>. 
/// </summary>
public readonly struct EntityRelations <TRelation>
    where TRelation : struct, IRelation
{
    private readonly AbstractEntityRelations relations;
    
    internal EntityRelations(AbstractEntityRelations relations) {
        this.relations = relations;
    }
    
    /// <summary>
    /// Returns a collection of entities having one or more relation.<br/>
    /// Executes in O(1).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when relations are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get all entities including their relations (the cartesian product aka CROSS JOIN) use<br/>
    ///     <see cref="EntityRelations{TRelation}.Pairs"/>
    ///   </item>
    /// </list>
    /// </remarks>
    public EntityReadOnlyCollection Entities => new EntityReadOnlyCollection(relations.store, relations.positionMap.Keys);
    
    /// <summary>
    /// Iterates all entity relations.<br/>
    /// Executes in O(N) N: number of all entity relations.
    /// </summary>
    public void For(ForEachEntity<TRelation> lambda) => relations.ForAllEntityRelations(lambda);
    
    /// <summary>
    /// Return all entity relations as pairs. A pair is <c>(entities[i], relations[i])</c><br/>
    /// Executes in O(1).  Most efficient way to iterate all entity relations.
    /// </summary>
    public (Entities entities, Chunk<TRelation> relations) Pairs => relations.GetAllEntityRelations<TRelation>();
}
