// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Friflo.Editor.Utils;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Utils;


// ReSharper disable HeuristicUnreachableCode
namespace Friflo.Editor.UI.Explorer;

public static class ExplorerCommands
{
    private static void Log(Func<string> message) {
        return;
#pragma warning disable CS0162 // Unreachable code detected
        var msg = message();
        Console.WriteLine(msg);
#pragma warning restore CS0162 // Unreachable code detected
    }
    
    /// <remarks>Not nice but <see cref="TreeDataGridCell.BeginEdit"/> is protected</remarks>
    internal static void RenameEntity(ExplorerTreeDataGrid grid)
    {
        var modelIndex  = grid.RowSelection!.SelectedIndex;
        var rowIndex    = grid.Rows!.ModelIndexToRowIndex(modelIndex);
        var row         = grid.TryGetRow(rowIndex);
        var cell        = EditorUtils.FindControl<TreeDataGridTextCell>(row);
        // hm...
        var beginEdit = typeof(TreeDataGridTextCell).GetMethod("BeginEdit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        beginEdit!.Invoke(cell, BindingFlags.Instance | BindingFlags.Public, null, null, null);
    }
    
    internal static void CopyItems(TreeSelection selection, InputElement element)
    {
        var entities    = selection.items.Select(item => item.Entity).ToList();
        var result      = TreeUtils.EntitiesToJsonArray(entities);
        var text        = result.entities.AsString();
        EditorUtils.CopyToClipboard(element, text);
        Focus(element);
    }
    
    internal static async void PasteItems(TreeSelection selection, ExplorerTreeDataGrid grid)
    {
        if (selection.Length > 0) {
            var dataEntities = await ClipboardUtils.GetDataEntities(grid);
            if (dataEntities != null) {
                var targetEntity    = selection.items[0].Entity;
                var targetPath      = grid.RowSelection!.SelectedIndex;
                if (!targetEntity.Parent.IsNull) {
                    targetEntity    = targetEntity.Parent; // add entities to parent
                    targetPath      = targetPath.Slice(0, targetPath.Count - 1);
                }
                var addResult       = TreeUtils.AddDataEntitiesToEntity(targetEntity, dataEntities);
                var indexes         = addResult.indexes.ToArray();
                var newSelection    = TreeIndexPaths.Create(targetPath, indexes);

                // requires Post() to avoid artifacts in grid. No clue why.
                StoreDispatcher.Post(() => {
                    grid.RowSelection.Clear();
                    newSelection.UpdateLeafIndexes(indexes);
                    grid.SelectItems(newSelection, SelectionView.Last, 0);    
                });
            }
        }
        Focus(grid);
    }
    
    internal static void DuplicateItems(TreeSelection selection, ExplorerTreeDataGrid grid)
    {
        if (selection.Length > 0) {
            var entities    = selection.items.Select(item => item.Entity).ToList();
            grid.GetSelectedPaths(out var selectedPaths);
            var indexes     = TreeUtils.DuplicateEntities(entities);
            
            // requires Post() to avoid artifacts in grid. No clue why.
            StoreDispatcher.Post(() => {
                grid.RowSelection?.Clear();
                selectedPaths.UpdateLeafIndexes(indexes);
                grid.SelectItems(selectedPaths, SelectionView.Last, 0);
            });
        }
        Focus(grid);
    }
    
    internal static void RemoveItems(TreeSelection selection, ExplorerTreeDataGrid grid)
    {
        var next = grid.GetSelectionPath();
        TreeUtils.RemoveExplorerItems(selection.items);
        grid.SetSelectionPath(next);
        Focus(grid);
    }
    
    internal static void CreateItems(TreeSelection selection, ExplorerTreeDataGrid grid)
    {
        grid.GetSelectedPaths(out var selectedPaths);
        var items   = selection.items;
        var indexes = new int[items.Length];
        for (int n = 0; n < items.Length; n++) {
            var item        = items[n];
            var parent      = item.Entity;
            var newEntity   = parent.Store.CreateEntity();
            Log(() => $"parent id: {parent.Id} - CreateEntity id: {newEntity.Id}");
            // newEntity.AddComponent(new EntityName($"entity"));
            indexes[n]      = parent.AddChild(newEntity);
        }
        grid.RowSelection?.Clear();
        foreach (var item in selection.items) {
            item.IsExpanded = true;
        }
        selectedPaths.AppendLeafIndexes(indexes);
        grid.SelectItems(selectedPaths, SelectionView.Last, 0);
        Focus(grid);
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsUp(TreeSelection selection, int shift, IInputElement element)
    {
        var indexes = TreeUtils.MoveExplorerItemsUp(selection.items, shift);
        Focus(element);
        return indexes;
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsDown(TreeSelection selection, int shift, IInputElement element)
    {
        var indexes = TreeUtils.MoveExplorerItemsDown(selection.items, shift);
        Focus(element);
        return indexes;
    }
    
    private static void Focus(IInputElement element) {
        element?.Focus();
    }
}
