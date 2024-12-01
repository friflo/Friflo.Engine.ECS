using Friflo.Engine.ECS.Relations;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct EntityRelation <TRelation>
    where TRelation : struct, IRelation
{
    private readonly AbstractEntityRelations relations;
    
    internal EntityRelation(AbstractEntityRelations relations) {
        this.relations = relations;
    }
    
    /// <summary>
    /// Returns a collection of entities having one or more relations of the specified <typeparamref name="TRelation"/> type.<br/>
    /// Executes in O(1).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when relations are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get all entities including their relations (the cartesian product aka CROSS JOIN) use<br/>
    ///     <see cref="RelationExtensions.GetAllEntityRelations{TRelation}"/>
    ///   </item>
    /// </list>
    /// </remarks>
    public EntityReadOnlyCollection Entities => new EntityReadOnlyCollection(relations.store, relations.positionMap.Keys);
    
    /// <summary>
    /// Iterates all entity relations of the specified <typeparamref name="TRelation"/> type.<br/>
    /// Executes in O(N) N: number of all entity relations.
    /// </summary>
    public void For(ForEachEntity<TRelation> lambda) => relations.ForAllEntityRelations(lambda);
}
