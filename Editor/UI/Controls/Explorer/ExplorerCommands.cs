// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Controls.Primitives;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;
using Friflo.Fliox.Engine.ECS.Serialize;

// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ParameterTypeCanBeEnumerable.Global
namespace Friflo.Fliox.Editor.UI.Controls.Explorer;

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
    
    internal static void CopyItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        var entities    = items.Select(item => item.Entity).ToList();
        var serializer  = new EntitySerializer();
        var stream      = new MemoryStream();
        serializer.WriteEntities(entities, stream);
        var text = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        EditorUtils.CopyToClipboard(grid, text);
        grid.FocusPanel();
    }
    
    internal static void RemoveItems(ExplorerItem[] items, ExplorerItem rootItem, ExplorerTreeDataGrid grid)
    {
        var next = grid.GetSelectionPath();
        foreach (var item in items) {
            var entity = item.Entity; 
            if (entity.TreeMembership != TreeMembership.treeNode) {
                continue;
            }
            if (rootItem == item) {
                continue;
            }
            var parent = entity.Parent;
            Log(() => $"parent id: {parent.Id} - Remove child id: {entity.Id}");
            parent.RemoveChild(entity);
        }
        grid.SetSelectionPath(next);
        grid.FocusPanel();
    }
    
    internal static void CreateItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        foreach (var item in items) {
            var parent      = item.Entity;
            var newEntity   = parent.Store.CreateEntity();
            Log(() => $"parent id: {parent.Id} - CreateEntity id: {newEntity.Id}");
            newEntity.AddComponent(new EntityName($"new entity-{newEntity.Id}"));
            parent.AddChild(newEntity);
        }
        grid.FocusPanel();
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsUp(ExplorerItem[] items, int shift, ExplorerTreeDataGrid grid)
    {
        var indexes = new List<int>(items.Length);
        var parent  = items[0].Entity.Parent;
        var pos     = 0;
        foreach (var item in items)
        {
            var entity      = item.Entity;
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index - shift;
            if (newIndex < pos++) {
                continue;
            }
            indexes.Add(newIndex);
            Log(() => $"parent id: {parent.Id} - Move child: ChildIds[{newIndex}] = {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        grid.FocusPanel();
        return indexes.ToArray();
    }
    
    /// <summary>Return the child indexes of moved items. Is empty if no item was moved.</summary>
    internal static int[] MoveItemsDown(ExplorerItem[] items, int shift, ExplorerTreeDataGrid grid)
    {
        var indexes     = new List<int>(items.Length);
        var parent      = items[0].Entity.Parent;
        var childCount  = parent.ChildCount;
        var pos     = 0;
        for (int n = items.Length - 1; n >= 0; n--)
        {
            var entity      = items[n].Entity;
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index + shift;
            if (newIndex >= childCount - pos++) {
                continue;
            }
            indexes.Add(newIndex);
            Log(() => $"parent id: {parent.Id} - Move child: ChildIds[{newIndex}] = {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        grid.FocusPanel();
        return indexes.ToArray();
    }
}