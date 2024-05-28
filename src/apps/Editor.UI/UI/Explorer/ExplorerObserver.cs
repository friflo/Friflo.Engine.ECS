// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using Avalonia.Controls;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Collections;

// ReSharper disable ConvertToConstant.Local
namespace Friflo.Editor.UI.Explorer;

internal class ExplorerObserver : EditorObserver
{
    private readonly    ExplorerTreeDataGrid    grid;
    private             ExplorerItemTree        tree;
    private static      int                     _treeCount;
        
    internal ExplorerObserver (ExplorerTreeDataGrid grid, AppEvents appEvents) : base (appEvents) { this.grid = grid; }
        
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
        var root        = tree.RootItem; 
        root.IsExpanded = true; 
        source.Items    = new []{ root };

        store.OnComponentAdded      += PostEntityUpdate;
        store.OnComponentRemoved    += PostEntityUpdate;
        store.OnEntitiesChanged     += EntitiesChanged;
        // select RootItem
        grid.RowSelection!.Select(0);
    }
    
    private void PostEntityUpdate(ComponentChanged args)
    {
        if (args.ComponentType.Type != typeof(EntityName)) {
            return;
        }
        if (!tree.TryGetItem(args.EntityId, out var item)) {
            return;
        }
        StoreDispatcher.Post(() => {
            if (Log) Console.WriteLine($"tree: {tree} - name update {_count++}");
            
            var changedEventArgs = new PropertyChangedEventArgs("name");
            item.propertyChangedHandler?.Invoke(grid, changedEventArgs);
        });
    }
    
    private void EntitiesChanged(object _, EntitiesChanged args)
    {
        foreach (var id in args.EntityIds) {
            if (!tree.TryGetItem(id, out var item)) {
                return;
            }
            // could Post() change events instead
            var propertyChangedEventArgs = new PropertyChangedEventArgs("name");
            item.propertyChangedHandler?.Invoke(grid, propertyChangedEventArgs);
        }
    }

    private static readonly bool    Log     = true;
    private static          int     _count;
}
