// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal struct RawEntity
{
    
#region internal fields
    [Browse(Never)] internal            int         archIndex;  // 4    could be short. if changing check perf
    [Browse(Never)] internal            int         compIndex;  // 4

    #endregion
    

}
