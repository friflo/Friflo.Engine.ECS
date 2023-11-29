// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable ConvertToConstant.Local
namespace Friflo.Fliox.Editor.UI.Explorer;

internal class ExplorerObserver : EditorObserver
{
    private readonly    ExplorerTreeDataGrid    grid;
    private             ExplorerItemTree        tree;
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
        tree            = new ExplorerItemTree(rootEntity, $"tree-{_treeCount++}");
        source.Items    = new []{ tree.RootItem };
        
        store.ComponentAddedHandler     += PostEntityUpdate;
        store.ComponentRemovedHandler   += PostEntityUpdate;
    }
    
    private void PostEntityUpdate(in ComponentChangedArgs args)
    {
        if (args.componentType.type != typeof(EntityName)) {
            return;
        }
        if (!tree.TryGetItem(args.entityId, out var item)) {
            return;
        }
        EditorUtils.Post(() => {
            if (Log) Console.WriteLine($"tree: {tree} - name update {_count++}");
            
            var args = new PropertyChangedEventArgs("name");
            item.propertyChangedHandler?.Invoke(grid, args);
        });
    }

    private static readonly bool    Log     = true;
    private static          int     _count;
}
