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
            var store       = items[0].Entity.Store;
            var entities    = items.Select(item => item.Entity).ToList();
            foreach (var entity in entities) {
                var parent  = entity.Parent;
                if (parent == null) {
                    continue;
                }
                var clone = store.CloneEntity(entity);
                parent.AddChild(clone);
            }
        }
        grid.FocusPanel();
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
    
    internal static async void PasteItems(ExplorerItem[] items, ExplorerTreeDataGrid grid)
    {
        var text = await EditorUtils.GetClipboardText(grid);
        if (text != null && items.Length > 0) {
            PasteEntities(items, text);
        }
        grid.FocusPanel();
    }
    
    private static void PasteEntities(ExplorerItem[] items, string text)
    {
        var serializer      = new EntitySerializer();
        var utf8            = Encoding.UTF8.GetBytes(text);
        var stream          = new MemoryStream(utf8.Length);
        stream.Write(utf8, 0, utf8.Length);
        stream.Position     = 0;
        var dataEntities    = new List<DataEntity>();
        var result          = serializer.ReadEntities(dataEntities, stream);
        // Console.WriteLine($"Paste - entities: {result.entityCount}, error: {result.error}");
        if (result.error != null) {
            return;
        }
        var pidMap          = new Dictionary<long, long>();
        var targetEntity    = items[0].Entity;
        targetEntity        = targetEntity.Parent ?? targetEntity; // paste entities to parent
        var store           = targetEntity.Store;
        foreach (var dataEntity in dataEntities)
        {
            var entity              = store.CreateEntity();
            var newPid              = store.GetNodeById(entity.Id).Pid;
            pidMap[dataEntity.pid]  = newPid;
            dataEntity.pid          = newPid;
        }
        var converter = EntityConverter.Default;
        foreach (var dataEntity in dataEntities)
        {
            var children = dataEntity.children;
            if (children != null) {
                for (int n = 0; n < children.Count; n++) {
                    var oldPid  = children[0];
                    var newPid  = pidMap[oldPid];
                    children[n] = newPid;     
                }
            }
            var entity = converter.DataEntityToEntity(dataEntity, store, out _);
            targetEntity.AddChild(entity);
        }
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