// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Friflo.Fliox.Editor.Utils;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable HeuristicUnreachableCode
namespace Friflo.Fliox.Editor.UI.Explorer;

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
        var json        = ECSUtils.EntitiesToJsonArray(entities);
        var text        = json.AsString();
        EditorUtils.CopyToClipboard(element, text);
        Focus(element);
    }
    
    internal static async void PasteItems(TreeSelection selection, InputElement element)
    {
        if (selection.Length > 0) {
            var dataEntities = await ClipboardUtils.GetDataEntities(element);
            if (dataEntities != null) {
                var targetEntity    = selection.items[0].Entity;
                targetEntity        = targetEntity.Parent ?? targetEntity; // add entities to parent
                ECSUtils.AddDataEntitiesToEntity(targetEntity, dataEntities);
            }
        }
        Focus(element);
    }
    
    internal static void DuplicateItems(TreeSelection selection, IInputElement element)
    {
        if (selection.Length > 0) {
            var entities    = selection.items.Select(item => item.Entity).ToList();
            ECSUtils.DuplicateEntities(entities);
        }
        Focus(element);
    }
    
    internal static void RemoveItems(TreeSelection selection, ExplorerItem rootItem, ExplorerTreeDataGrid grid)
    {
        var next = grid.GetSelectionPath();
        ECSUtils.RemoveExplorerItems(selection.items, rootItem);
        grid.SetSelectionPath(next);
        Focus(grid);
    }
    
    internal static void CreateItems(TreeSelection selection, IInputElement element)
    {
        foreach (var item in selection.items) {
            var parent      = item.Entity;
            var newEntity   = parent.Store.CreateEntity();
            Log(() => $"parent id: {parent.Id} - CreateEntity id: {newEntity.Id}");
            newEntity.AddComponent(new EntityName($"entity"));
            parent.AddChild(newEntity);
        }
        Focus(element);
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsUp(TreeSelection selection, int shift, IInputElement element)
    {
        var indexes = ECSUtils.MoveExplorerItemsUp(selection.items, shift);
        Focus(element);
        return indexes.ToArray();
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsDown(TreeSelection selection, int shift, IInputElement element)
    {
        var indexes = ECSUtils.MoveExplorerItemsDown(selection.items, shift);
        Focus(element);
        return indexes.ToArray();
    }
    
    private static void Focus(IInputElement element) {
        element?.Focus();
    }
}
