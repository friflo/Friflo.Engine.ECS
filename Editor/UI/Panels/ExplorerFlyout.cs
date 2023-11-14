using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Friflo.Fliox.Editor.UI.Explorer;

// ReSharper disable SuggestBaseTypeForParameter
namespace Friflo.Fliox.Editor.UI.Panels;

public class ExplorerFlyout : MenuFlyout
{
    private readonly    ExplorerPanel       explorer;
    
    internal ExplorerFlyout(ExplorerPanel explorer)
    {
        this.explorer   = explorer;
        this.FlyoutPresenterClasses.Add("editorMenuFlyout");
        var menuItem1 = new MenuItem {
            Header = "Standard"
        };
        Items.Add(menuItem1);
        base.OnOpened();
    }
    
    protected override void OnOpened() {

        var selection   = explorer.DragDrop.RowSelection;
        if (selection != null) {
            // var firstSelected   = (ExplorerItem)selection.SelectedItem;
            var items           = (IReadOnlyList<ExplorerItem>)selection.SelectedItems;
            AddMenuItems(items);
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
    private void AddMenuItems(IReadOnlyList<ExplorerItem> selectedItems)
    {
        var items       = selectedItems.ToArray();
        DeleteEntity(items);
        NewEntity   (items);
    }
    
    private void DeleteEntity(ExplorerItem[] items)
    {
        var rootItem    = explorer.RootItem;
        var isRootItem  = items.Length == 1 && items[0] == rootItem;
        var canDelete   = isRootItem ? items.Length > 1 : items.Length > 0;
        var deleteMenu  = new MenuItem { Header = "Delete entity", IsEnabled = canDelete };
        if (canDelete) {
            deleteMenu.Click += (_, _) => ExplorerCommands.RemoveItems(items, rootItem);
        }
        Items.Add(deleteMenu);
    }
    
    private void NewEntity(ExplorerItem[] items)
    {
        var newMenu  = new MenuItem { Header = "New entity", IsEnabled = items.Length > 0 };
        if (items.Length > 0) {
            newMenu.Click += (_, _) => ExplorerCommands.CreateItems(items);
        }
        Items.Add(newMenu);
    }
}
