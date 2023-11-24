// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable UseIndexFromEndExpression
namespace Friflo.Fliox.Editor.UI.Controls.Explorer;

internal enum SelectionView
{
    First   = 0,
    Last    = 1,
}

internal class MoveSelection
{
    internal readonly   IndexPath   parent;
    internal readonly   IndexPath   first;
    internal readonly   IndexPath   last;

    public   override   string      ToString() => parent.ToString();

    private MoveSelection(in IndexPath parent, in IndexPath first, in IndexPath last) {
        this.parent     = parent;
        this.first      = first;
        this.last       = last;
    }
    
    internal static MoveSelection Create(IReadOnlyList<IndexPath> indexes)
    {
        if (indexes.Count == 0) {
            return null;
        }
        var first   = indexes[0];
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
        return new MoveSelection(parent, indexes[0], indexes[indexes.Count - 1]);
    }
    
    internal static MoveSelection Create(in IndexPath parent, int[] indexes)
    {
        var first   = parent.Append(indexes[0]);
        var last    = parent.Append(indexes[indexes.Length - 1]);
        return new MoveSelection(parent, first, last);
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