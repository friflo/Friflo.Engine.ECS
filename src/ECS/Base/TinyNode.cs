// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static Friflo.Fliox.Engine.ECS.NodeFlags;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;


public struct TinyNode
{
#region public properties
    /// <summary>Unique id within an <see cref="EntityNode"/> instance</summary>
                    public              int                 Id          =>  id;
    /// <summary>Permanent unique pid used for persistence of an entity in a database </summary>
                    public              long                Pid         =>  pid;
                    public              NodeFlags           Flags       =>  flags;
                    
                    public   override   string              ToString()  => GetString();
    #endregion
    
 #region internal fields
    [Browse(Never)] internal readonly   int         id;         // 4
    [Browse(Never)] internal            long        pid;        // 8
    [Browse(Never)] internal            NodeFlags   flags;      // 4 (1)
    
    [Browse(Never)] internal            short       archIndex;  // 4    for 'GameEntity free usage'
    [Browse(Never)] internal            int         compIndex;  // 4    for 'GameEntity free usage'

                    
                    internal            bool        Is      (NodeFlags flag) => (flags & flag) != 0;
                    internal            bool        IsNot   (NodeFlags flag) => (flags & flag) == 0;
    #endregion
    
#region internal methods
    internal TinyNode(int id) {
        this.id     = id;
    }
    
    private string GetString()
    {
        var sb = new StringBuilder();

        sb.Append("id: ");
        sb.Append(id);

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
