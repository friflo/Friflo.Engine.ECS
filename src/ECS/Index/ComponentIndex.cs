using System.Collections.Generic;
using Friflo.Engine.ECS.Index;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// The index for <see cref="IIndexedComponent{TValue}"/> struct's to search entities with a specific component value in O(1).<br/>
/// An instance is returned via <see cref="IndexExtensions.ComponentIndex{TIndexedComponent,TValue}"/>. 
/// </summary>
public readonly struct ComponentIndex <TIndexedComponent,TValue>
    where TIndexedComponent : struct, IIndexedComponent<TValue>
{
    private readonly GenericComponentIndex<TValue> index;
    
    internal ComponentIndex(GenericComponentIndex<TValue> index) {
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
    
    public override string ToString() => $"Count: {Values.Count}";
}
