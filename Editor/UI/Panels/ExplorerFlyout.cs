using System;
using Avalonia.Controls;
using Friflo.Fliox.Editor.UI.Explorer;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Panels;

public class ExplorerFlyout : MenuFlyout
{
    private readonly ExplorerPanel explorer;
    
    public ExplorerFlyout(ExplorerPanel explorer)
    {
        this.explorer = explorer;
        var menuItem1 = new MenuItem {
            Header = "Standard"
        };
        Items.Add(menuItem1);
        base.OnOpened();
    }

    protected override void OnOpened() {

        var selection   = explorer.DragDrop.RowSelection;
        if (selection != null) {
            var item        = (ExplorerItem)selection.SelectedItem;
            if (item != null) {
                AddMenuItems(item);
            }
        }
        base.OnOpened();
    }

    protected override void OnClosed() {
        for (int n = Items.Count - 1; n >= 1; n--) {
            Items.RemoveAt(n);
        }
        base.OnClosed();
    }
    
    private void AddMenuItems(ExplorerItem item)
    {
        // --- Delete entity
        var deleteMenu  = new MenuItem { Header = "Delete entity" };
        deleteMenu.Click += (_, _) => {
            Console.WriteLine($"Delete: {item.Name ?? $"entity {item.Id}"}");
            item.Entity.DeleteEntity();
        };
        Items.Add(deleteMenu);

        // --- New entity
        var newMenu  = new MenuItem { Header = "New entity" };
        newMenu.Click += (_, _) => {
            Console.WriteLine($"New: {item.Name ?? $"entity {item.Id}"}");
            var newEntity = item.Entity.Store.CreateEntity();
            newEntity.AddComponent(new EntityName($"new entity-{newEntity.Id}"));
            item.Entity.AddChild(newEntity);
        };
        Items.Add(newMenu);
    }
}
