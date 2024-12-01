using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct ComponentIndex <TIndexedComponent,TValue>
    where TIndexedComponent : struct, IIndexedComponent<TValue>
{
    private readonly AbstractComponentIndex<TValue> index;
    
    internal ComponentIndex(AbstractComponentIndex<TValue> index) {
        this.index = index;
    }
    
    /// <summary>
    /// Return the entities having a component with the passed component value.<br/>
    /// Executes in O(1) with default index. 
    /// </summary>
    public Entities this[TValue value] => index.GetHasValueEntities(value);
    
    /// <summary>
    /// Returns all indexed component values of the passed <typeparamref name="TIndexedComponent"/> type.<br/>
    /// Executes in O(1). Each value in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when indexed component values are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get the entities having a specific component value use <see cref="this[TValue]"/>.
    ///   </item>
    ///   <item>
    ///     If <typeparamref name="TValue"/> is a class all collection values are not null.<br/>
    ///     Use <see cref="this[TValue]"/> to check if null is indexed.
    ///   </item>
    /// </list>
    /// </remarks>
    public IReadOnlyCollection<TValue> Values => index.IndexedComponentValues;
    
    /// <summary>
    /// Returns all entities linked by the specified <see cref="ILinkComponent"/> type.<br/>
    /// Executes in O(1). Each entity in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when component link values are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get the entities linking a specific entity use <see cref="IndexExtensions.GetIncomingLinks{TComponent}"/>.<br/>
    ///   </item>
    ///   <item>
    ///     The method is a specialized version of <see cref="Values"/><br/>
    ///     using <c> TIndexedComponent = ILinkComponent</c> and <c>TValue = Entity</c>.  
    ///   </item>
    /// </list>
    /// </remarks>
    public IReadOnlyCollection<Entity> LinkedEntities => ((AbstractComponentIndex<Entity>)(object)index).IndexedComponentValues;
    
    public override string ToString() => $"Count: {Values.Count}";
}