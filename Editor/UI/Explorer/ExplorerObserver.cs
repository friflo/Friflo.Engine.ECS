// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS.Collections;

namespace Friflo.Fliox.Editor.UI.Explorer;

internal class ExplorerObserver : EditorObserver
{
    private readonly    ExplorerTreeDataGrid    grid;
    private static      int                     _treeCount;
        
    internal ExplorerObserver (ExplorerTreeDataGrid grid, Editor editor) : base (editor) { this.grid = grid; }
        
    /// <summary>
    /// Set <see cref="HierarchicalTreeDataGridSource{TModel}.Items"/> of <see cref="ExplorerViewModel.ExplorerItemSource"/>
    /// </summary>
    protected override void OnEditorReady()
    {
        var store       = Store;
        if (store == null) throw new InvalidOperationException("expect Store is present");
        // return;
        var source      = grid.GridSource;
        var rootEntity  = store.StoreRoot;
        var tree        = new ExplorerItemTree(rootEntity, $"tree-{_treeCount++}");
        source.Items    = new []{ tree.RootItem };
    }
}
