using System;
using System.Collections.Generic;
using Friflo.Fliox.Editor.UI.Explorer;
using Friflo.Fliox.Engine.ECS;

// ReSharper disable ParameterTypeCanBeEnumerable.Global
namespace Friflo.Fliox.Editor.UI.Panels;

public static class ExplorerCommands
{
    internal static void RemoveItems(ExplorerItem[] items, ExplorerItem rootItem, ExplorerTreeDataGrid panel)
    {
        foreach (var item in items) {
            var entity = item.Entity; 
            if (entity.TreeMembership != TreeMembership.treeNode) {
                continue;
            }
            if (rootItem == item) {
                continue;
            }
            var parent = entity.Parent;
            Console.WriteLine($"parent id: {parent.Id} - Remove child id: {entity.Id}");
            parent.RemoveChild(entity);
        }
        panel.FocusPanel();
    }
    
    internal static void CreateItems(ExplorerItem[] items, ExplorerTreeDataGrid panel)
    {
        foreach (var item in items) {
            var parent      = item.entity;
            var newEntity   = parent.Store.CreateEntity();
            Console.WriteLine($"parent id: {parent.Id} - New child id: {newEntity.Id}");
            newEntity.AddComponent(new EntityName($"new entity-{newEntity.Id}"));
            parent.AddChild(newEntity);
        }
        panel.FocusPanel();
    }
    
    internal static int[] MoveItemsUp(ExplorerItem[] items, int shift, ExplorerTreeDataGrid panel)
    {
        var indexes = new List<int>(items.Length);
        foreach (var item in items)
        {
            var entity      = item.entity;
            var parent      = entity.Parent;
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index - shift;
            if (newIndex < 0) {
                continue;
            }
            indexes.Add(newIndex);
            Console.WriteLine($"parent id: {parent.Id} - Move child id: {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        panel.FocusPanel();
        return indexes.ToArray();
    }
    
    internal static int[] MoveItemsDown(ExplorerItem[] items, int shift, ExplorerTreeDataGrid panel)
    {
        var indexes = new List<int>(items.Length);
        for (int n = items.Length - 1; n >= 0; n--)
        {
            var item        = items[n]; 
            var entity      = item.entity;
            var parent      = entity.Parent;
            int index       = parent.GetChildIndex(entity.Id);
            int newIndex    = index + shift;
            if (newIndex >= parent.ChildCount) {
                continue;
            }
            indexes.Add(newIndex);
            Console.WriteLine($"parent id: {parent.Id} - Move child id: {entity.Id}");
            parent.InsertChild(newIndex, entity);
        }
        panel.FocusPanel();
        return indexes.ToArray();
    }
}