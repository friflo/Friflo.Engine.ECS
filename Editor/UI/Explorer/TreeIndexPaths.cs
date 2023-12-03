// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable UseIndexFromEndExpression
namespace Friflo.Fliox.Editor.UI.Explorer;

internal enum SelectionView
{
    First   = 0,
    Last    = 1,
}

internal class TreeIndexPaths
{
    internal            IndexPath       First   => paths[0];
    internal readonly   IndexPath[]     paths;

    public   override   string          ToString() => $"Length: {paths.Length}";

    private TreeIndexPaths(IndexPath[] paths) {
        this.paths      = paths;
    }
    
    internal static TreeIndexPaths Create(IReadOnlyList<IndexPath> indexes)
    {
        if (indexes.Count == 0) {
            return null;
        }
        var first = indexes[0];
        if (first == new IndexPath(0)) {
            return null;
        }
        var paths = indexes.ToArray();
        return new TreeIndexPaths(paths);
    }
    
    internal static TreeIndexPaths Create(in IndexPath parent, int[] indexes)
    {
        var paths = new IndexPath[indexes.Length];
        for (int n = 0; n < indexes.Length; n++) {
            paths[n] = parent.Append(indexes[n]); 
        }
        return new TreeIndexPaths(paths);
    }
}
//cannot use TreeDataGridRow targetRow
internal struct RowDropContext
{

    /// <summary>
    /// Cannot use TreeDataGridRow as <see cref="targetModel"/>.<br/>
    /// It seems <see cref="TreeDataGridRow"/>'s exists only when in view.
    /// </summary>
    internal    IndexPath       targetModel;
    internal    ExplorerItem[]  droppedItems;
}