// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static Friflo.Fliox.Engine.ECS.NodeFlags;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal struct TinyEntity
{
    
#region internal fields
    [Browse(Never)] internal            NodeFlags   flags;      // 1
    
    [Browse(Never)] internal            short       archIndex;  // 2    for 'GameEntity free usage'
    [Browse(Never)] internal            int         compIndex;  // 4    for 'GameEntity free usage'

                    public   override   string              ToString()  => GetString();
                    internal            bool        Is      (NodeFlags flag) => (flags & flag) != 0;
                    internal            bool        IsNot   (NodeFlags flag) => (flags & flag) == 0;
    #endregion
    
#region internal methods
    private string GetString()
    {
        var sb = new StringBuilder();

        sb.Append("TinyEntity - ");

        if (flags != 0) {
            sb.Append("  flags: ");
            var startPos = sb.Length;
            if (Is(TreeNode)) {
                sb.Append("TreeNode");
            }
            if (Is(Created)) {
                if (startPos < sb.Length) {
                    sb.Append(" | ");
                }
                sb.Append("Created");
            }
        }
        return sb.ToString();
    }
    #endregion
}
