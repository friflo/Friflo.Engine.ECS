// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// Information of a matching system returned by <see cref="SystemExtensions.GetMatchingSystems"/>.
/// </summary>
public struct SystemMatch
{
#region properties
    /// <summary> The matching system. </summary>
    public          BaseSystem  System      => system;

    /// <summary> The depth of a matching system. </summary>
    public          int         Depth       => depth;

    /// <summary> The number of matching systems within a <see cref="SystemGroup"/>. </summary>
    public          int         Count       => count;

    public override string      ToString() => GetString(); 
    #endregion
    
#region internal fields
    internal    int         parent;
    internal    int         count;
    internal    BaseSystem  system;
    internal    int         depth;
    #endregion
        
#region methods
    private string GetString() {
        if (system is SystemGroup) {
            return $"{System.Name} [{Count}] - Depth: {Depth}";
        }
        return $"{System.Name} - Depth: {Depth}";
    }
    #endregion
}
