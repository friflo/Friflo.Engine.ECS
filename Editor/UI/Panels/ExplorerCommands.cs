using System;
using Friflo.Fliox.Editor.UI.Explorer;
using Friflo.Fliox.Engine.ECS;

// ReSharper disable ParameterTypeCanBeEnumerable.Global
namespace Friflo.Fliox.Editor.UI.Panels;

public static class ExplorerCommands
{
    internal static void RemoveItems(ExplorerItem[] items, ExplorerItem rootItem)
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
    }
    
    internal static void CreateItems(ExplorerItem[] items)
    {
        foreach (var item in items) {
            var entity      = item.entity;
            var newEntity   =  entity.Store.CreateEntity();
            Console.WriteLine($"parent id: {entity.Id} - New child id: {newEntity.Id}");
            newEntity.AddComponent(new EntityName($"new entity-{newEntity.Id}"));
            entity.AddChild(newEntity);
        }
    }
}