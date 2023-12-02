// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Controls.Primitives;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;

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
    
    internal static void DuplicateItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        Console.WriteLine("Duplicate");
        if (items.Length > 0) {
            var entities    = items.Select(item => item.Entity).ToList();
            ECSUtils.DuplicateEntities(entities);
        }
        grid.FocusPanel();
    }
    
    internal static void CopyItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        var entities    = items.Select(item => item.Entity).ToList();
        var json        = ECSUtils.EntitiesToJsonArray(entities);
        var text        = json.AsString();
        EditorUtils.CopyToClipboard(grid, text);
        grid.FocusPanel();
    }
    
    internal static async void PasteItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        var text = await EditorUtils.GetClipboardText(grid);
        if (text != null && items.Length > 0) {
            var jsonArray       = new JsonValue(Encoding.UTF8.GetBytes(text));
            var dataEntities    = new List<DataEntity>();
            if (ECSUtils.JsonArrayToDataEntities (jsonArray, dataEntities) == null) {
                var targetEntity    = items[0].Entity;
                targetEntity        = targetEntity.Parent ?? targetEntity; // add entities to parent
                ECSUtils.AddDataEntitiesToEntity(targetEntity, dataEntities);
            }
        }
        grid.FocusPanel();
    }
    
    internal static void RemoveItems(ExplorerItem[] items, ExplorerItem rootItem, ExplorerTreeDataGrid grid)
    {
        var next = grid.GetSelectionPath();
        ECSUtils.RemoveExplorerItems(items, rootItem);
        grid.SetSelectionPath(next);
        grid.FocusPanel();
    }
    
    internal static void CreateItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        foreach (var item in items) {
            var parent      = item.Entity;
            var newEntity   = parent.Store.CreateEntity();
            Log(() => $"parent id: {parent.Id} - CreateEntity id: {newEntity.Id}");
            newEntity.AddComponent(new EntityName($"entity"));
            parent.AddChild(newEntity);
        }
        grid.FocusPanel();
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsUp(ExplorerItem[] items, int shift, ExplorerTreeDataGrid grid)
    {
        var indexes = ECSUtils.MoveExplorerItemsUp(items, shift);
        grid.FocusPanel();
        return indexes.ToArray();
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsDown(ExplorerItem[] items, int shift, ExplorerTreeDataGrid grid)
    {
        var indexes = ECSUtils.MoveExplorerItemsDown(items, shift);
        grid.FocusPanel();
        return indexes.ToArray();
    }
}