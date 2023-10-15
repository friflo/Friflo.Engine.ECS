// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal struct RawEntity
{
    
#region internal fields
    [Browse(Never)] internal            short       archIndex;  // 2    for 'GameEntity free usage'
    [Browse(Never)] internal            int         compIndex;  // 4    for 'GameEntity free usage'

    #endregion
    

}
