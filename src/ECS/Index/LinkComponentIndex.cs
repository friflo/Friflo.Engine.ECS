using System.Collections.Generic;
using Friflo.Engine.ECS.Index;
// ReSharper disable UnusedTypeParameter

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// The index for <see cref="ILinkComponent"/> struct's to search entities with a specific entity in O(1).
/// </summary>
public readonly struct LinkComponentIndex <TLinkComponent>
    where TLinkComponent: struct, ILinkComponent
{
    private readonly GenericComponentIndex<Entity> index;
    
    internal LinkComponentIndex(GenericComponentIndex<Entity> index) {
        this.index = index;
    }
    
    /// <summary>
    /// Return the entities having a link component with the passed component value.<br/>
    /// Executes in O(1) with default index. 
    /// </summary>
    public Entities this[Entity value] => index.GetHasValueEntities(value);
    
    /// <summary>
    /// Returns all indexed link component values of the passed<see cref="Entity"/> type.<br/>
    /// Executes in O(1). Each value in the returned list is unique. See remarks for additional infos.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     The returned collection changes when indexed component values are updated, removed or added.
    ///   </item>
    ///   <item>
    ///     To get the entities having a specific component value use <see cref="this[Entity]"/>.
    ///   </item>
    /// </list>
    /// </remarks>
    public IReadOnlyCollection<Entity> Values => index.IndexedComponentValues;
    
    public override string ToString() => $"Count: {Values.Count}";
}