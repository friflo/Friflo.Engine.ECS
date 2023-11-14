using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Friflo.Fliox.Editor.UI.Explorer;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Panels;

public class ExplorerFlyout : MenuFlyout
{
    private readonly ExplorerPanel      explorer;
    
    internal ExplorerFlyout(ExplorerPanel explorer)
    {
        this.explorer   = explorer;
        var menuItem1 = new MenuItem {
            Header = "Standard"
        };
        Items.Add(menuItem1);
        base.OnOpened();
    }

    protected override void OnOpened() {

        var selection   = explorer.DragDrop.RowSelection;
        if (selection != null) {
            var firstSelected   = (ExplorerItem)selection.SelectedItem;
            var items           = (IReadOnlyList<ExplorerItem>)selection.SelectedItems;
            AddMenuItems(firstSelected, items);
        }
        base.OnOpened();
    }

    protected override void OnClosed() {
        for (int n = Items.Count - 1; n >= 1; n--) {
            Items.RemoveAt(n);
        }
        base.OnClosed();
    }
    
    private void AddMenuItems(ExplorerItem firstSelected, IReadOnlyList<ExplorerItem> selectedItems)
    {
        var firstEntity = firstSelected?.Entity;
        var items = selectedItems.ToArray();
        // --- Delete entity
        bool isRootItem     = items.Length == 1 && items[0].Entity.Store.StoreRoot ==  items[0].Entity;
        var deleteMenu      = new MenuItem { Header = "Delete entity", IsEnabled = !isRootItem };
        if (!isRootItem) {
            deleteMenu.Click += (_, _) => {
                foreach (var item in items) {
                    var entity = item.Entity; 
                    if (entity.TreeMembership != TreeMembership.treeNode) {
                        continue;
                    }
                    Console.WriteLine($"Remove entity id: {entity.Id}");
                    entity.Parent.RemoveChild(entity);
                }
            };
        }
        Items.Add(deleteMenu);

        // --- New entity
        var newMenu  = new MenuItem { Header = "New entity", IsEnabled = firstEntity != null };
        if (firstEntity != null) {
            newMenu.Click += (_, _) => {
                var newEntity =  firstEntity.Store.CreateEntity();
                Console.WriteLine($"parent id: { firstEntity.Id} - New child id: {newEntity.Id}");
                newEntity.AddComponent(new EntityName($"new entity-{newEntity.Id}"));
                firstEntity.AddChild(newEntity);
            };
        }
        Items.Add(newMenu);
    }
}
