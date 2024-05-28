// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Engine.ECS.Collections;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable ReplaceSliceWithRangeIndexer
// ReSharper disable SuggestBaseTypeForParameter
namespace Friflo.Editor.UI.Explorer;

public class ExplorerFlyout : MenuFlyout
{
    private readonly    ExplorerTreeDataGrid       grid;
    
    internal ExplorerFlyout(ExplorerTreeDataGrid grid)
    {
        this.grid   = grid;
        FlyoutPresenterClasses.Add("editorMenuFlyout");
        var menu    = new MenuItem { Header = "Test" };
        menu.Click += ExecuteTestCommand;
        Items.Add(menu);
        base.OnOpened();
    }
    
    private void ExecuteTestCommand(object sender, RoutedEventArgs args) {
        Console.WriteLine("Explorer - Test");
        // add TreeDataGrid column on demand
        var idCol = new TextColumn<ExplorerItem, int>   ("id'",   item => item.Id,   GridLength.Auto);
        grid.GridSource.Columns.Add(idCol);
    }
    
    protected override void OnOpened() {

        var rowSelection = grid.RowSelection;
        if (rowSelection != null) {
            // var firstSelected   = (ExplorerItem)selection.SelectedItem;
            var selection   = grid.GetSelection();
            var rootItem    = grid.RootItem;
            grid.GetSelectedPaths(out var selectedPaths);
            AddMenuItems(selection, selectedPaths, rootItem);
        }
        base.OnOpened();
    }

    protected override void OnClosed() {
        for (int n = Items.Count - 1; n >= 1; n--) {
            Items.RemoveAt(n);
        }
        base.OnClosed();
    }
    
    // ----------------------------------- add menu commands -----------------------------------
    private void AddMenuItems(
        TreeSelection       selection,
        TreeIndexPaths      selectedPaths,
        ExplorerItem        rootItem)
    {
        RenameEntity        (selection);
        DuplicateEntities   (selection);
        DeleteEntities      (selection, rootItem);
        Items.Add(new Separator());
        
        CopyEntities        (selection);
        PasteEntities       (selection);
        NewEntity           (selection);
        
        if (selectedPaths != null) {
            Items.Add(new Separator());
            MoveEntityUp    (selection, selectedPaths);
            MoveEntityDown  (selection, selectedPaths);
        }
    }
    
    private void RenameEntity(TreeSelection selection)
    {
        var canRename       = selection.Length == 1;
        var menu            = new MenuItem { Header = "Rename", IsEnabled = canRename };
        menu.InputGesture   = new KeyGesture(Key.F2);
        menu.Click += (_, _) => ExplorerCommands.RenameEntity(grid);
        Items.Add(menu);
    }
    
    private void DuplicateEntities(TreeSelection selection)
    {
        var canDuplicate    = selection.Length > 0;
        var menu            = new MenuItem { Header = "Duplicate", IsEnabled = canDuplicate };
        menu.InputGesture   = new KeyGesture(Key.D, KeyModifiers.Control);
        menu.Click += (_, _) => ExplorerCommands.DuplicateItems(selection, grid);
        Items.Add(menu);
    }
    
    private void CopyEntities(TreeSelection selection)
    {
        var canCopy         = selection.Length > 0;
        var menu            = new MenuItem { Header = "Copy", IsEnabled = canCopy };
        menu.InputGesture   = new KeyGesture(Key.C, KeyModifiers.Control);
        menu.Click += (_, _) => ExplorerCommands.CopyItems(selection, grid);
        Items.Add(menu);
    }
    
    private void PasteEntities(TreeSelection selection)
    {
        var canPaste        = selection.Length > 0;
        var menu            = new MenuItem { Header = "Paste", IsEnabled = canPaste };
        menu.InputGesture   = new KeyGesture(Key.V, KeyModifiers.Control);
        menu.Click += (_, _) => ExplorerCommands.PasteItems(selection, grid);
        Items.Add(menu);
    }
    
    private void DeleteEntities(TreeSelection selection, ExplorerItem rootItem)
    {
        var items           = selection.items; 
        var isRootItem      = items.Length == 1 && items[0] == rootItem;
        var canDelete       = isRootItem ? items.Length > 1 : items.Length > 0;
        var menu            = new MenuItem { Header = "Delete", IsEnabled = canDelete };
        menu.InputGesture   = new KeyGesture(Key.Delete);
        if (canDelete) {
            menu.Click += (_, _) => ExplorerCommands.RemoveItems(selection, grid);
        }
        Items.Add(menu);
    }
    
    private void NewEntity(TreeSelection selection)
    {
        var menu            = new MenuItem { Header = "New entity", IsEnabled = selection.Length > 0 };
        menu.InputGesture   = new KeyGesture(Key.N, KeyModifiers.Control);
        if (selection.Length > 0) {
            menu.Click += (_, _) => ExplorerCommands.CreateItems(selection, grid);
        }
        Items.Add(menu);
    }
    
    private void MoveEntityUp(TreeSelection selection, TreeIndexPaths selectedPaths)
    {
        var canMove         = selection.Length > 1 || selectedPaths.First.Last() > 0;
        var menu            = new MenuItem { Header = "Move up", IsEnabled = canMove };
        menu.InputGesture   = new KeyGesture(Key.Up, KeyModifiers.Control);
        menu.Click += (_, _) => {
            var indexes = ExplorerCommands.MoveItemsUp(selection, 1, grid);
            selectedPaths.UpdateLeafIndexes(indexes);
            grid.SelectItems(selectedPaths, SelectionView.First, 1);
        };
        Items.Add(menu);
    }
    
    private void MoveEntityDown(TreeSelection selection, TreeIndexPaths selectedPaths)
    {
        var canMove = true;
        if (selection.Length == 1) {
            var entity  = selection.items.Last().Entity;
            var parent  = entity.Parent;
            if (!parent.IsNull) {
                var index   = parent.GetChildIndex(entity);
                canMove     = index < parent.ChildCount - 1;
            } else {
                canMove     = false;
            }
        }
        var menu            = new MenuItem { Header = "Move down", IsEnabled = canMove };
        menu.InputGesture   = new KeyGesture(Key.Down, KeyModifiers.Control);
        menu.Click += (_, _) => {
            var indexes = ExplorerCommands.MoveItemsDown(selection, 1, grid);
            selectedPaths.UpdateLeafIndexes(indexes);
            grid.SelectItems(selectedPaths, SelectionView.Last, 1);
        };
        Items.Add(menu);
    }
}
