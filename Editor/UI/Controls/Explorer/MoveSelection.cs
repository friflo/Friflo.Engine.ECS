using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Friflo.Fliox.Engine.ECS.Collections;

namespace Friflo.Fliox.Editor.UI.Controls.Explorer;

internal enum SelectionView
{
    First   = 0,
    Last    = 1,
}

internal class MoveSelection
{
    internal            IndexPath   parent;
    internal readonly   IndexPath[] indexes;

    public   override   string      ToString() => parent.ToString();

    private MoveSelection(in IndexPath parent, IndexPath[] indexes) {
        this.parent     = parent;
        this.indexes    = indexes;
    }
    
    internal static MoveSelection Create(IReadOnlyList<IndexPath> indexes)
    {
        if (indexes.Count == 0) {
            return null;
        }
        var first = indexes[0];
        if (first == new IndexPath(0)) {
            return null;
        }
        var parent  = indexes[0].Slice(0, first.Count - 1);
        for (int n = 1; n < indexes.Count; n++)
        {
            var index = indexes[n];
            if(!parent.IsParentOf(index)) {
                return null;
            }
        }
        var indexesCopy = indexes.ToArray(); // create copy to avoid side effects
        return new MoveSelection(parent, indexesCopy);
    }
    
    internal static MoveSelection Create(in IndexPath parent, int[] indexes)
    {
        var indexPaths = new IndexPath[indexes.Length];
        for (int n = 0; n < indexes.Length; n++)
        {
            var child       = parent.Append(indexes[n]);
            indexPaths[n]   = child;
        }
        return new MoveSelection(parent, indexPaths);
    }
}

internal struct RowDropContext
{
    internal    TreeDataGridRow targetRow;
    internal    ExplorerItem[]  droppedItems;
} 