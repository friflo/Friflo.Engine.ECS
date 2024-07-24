// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Friflo.Engine.ECS.Collections;

// ReSharper disable UseIndexFromEndExpression
namespace Friflo.Editor.UI.Explorer;

internal enum SelectionView
{
    First   = 0,
    Last    = 1,
}

internal class TreeIndexPaths
{
    internal            IndexPath       First   => paths[0];
    internal readonly   IndexPath[]     paths;

    public   override   string          ToString() => $"IndexPath[{paths.Length}]";

    private TreeIndexPaths(IndexPath[] paths) {
        this.paths      = paths;
    }
    
    internal static TreeIndexPaths Create(IReadOnlyList<IndexPath> indexes)
    {
        if (indexes.Count == 0) {
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
    
    internal void UpdateLeafIndexes(int[] indexes)
    {
        if (indexes == null) {
            return;
        }
        int length      = indexes.Length;
        var indexPaths  = paths;
        if (length != indexPaths.Length) throw new InvalidOperationException("expect equal lengths");
        
        for (int n = 0; n < length; n++) {
            var index       = indexes[n];
            if (index == -1) {
                continue; // skip if no index available. E.g. for Duplicate root
            }
            var path        = indexPaths[n];
            var parent      = path.Slice(0, path.Count - 1);
            indexPaths[n]   = parent.Append(index);
        }
    }
    
    internal void AppendLeafIndexes(int[] indexes)
    {
        int length      = indexes.Length;
        var indexPaths  = paths;
        if (length != indexPaths.Length) throw new InvalidOperationException("expect equal lengths");
        
        for (int n = 0; n < length; n++) {
            indexPaths[n] = indexPaths[n].Append(indexes[n]);
        }
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