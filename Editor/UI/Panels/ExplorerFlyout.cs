using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Friflo.Fliox.Editor.UI.Explorer;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable ReplaceSliceWithRangeIndexer
// ReSharper disable SuggestBaseTypeForParameter
namespace Friflo.Fliox.Editor.UI.Panels;

public class ExplorerFlyout : MenuFlyout
{
    private readonly    ExplorerTreeDataGrid       grid;
    
    internal ExplorerFlyout(ExplorerPanel explorer)
    {
        grid   = explorer.Grid;
        FlyoutPresenterClasses.Add("editorMenuFlyout");
        var menuItem1 = new MenuItem {
            Header = "Standard"
        };
        Items.Add(menuItem1);
        base.OnOpened();
    }
    
    protected override void OnOpened() {

        var selection   = grid.RowSelection;
        if (selection != null) {
            // var firstSelected   = (ExplorerItem)selection.SelectedItem;
            var selectedItems   = grid.GetSelectedItems();
            var rootItem        = grid.RootItem;
            grid.GetMoveSelection(out var moveSelection);
            AddMenuItems(selectedItems, moveSelection, rootItem);
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
    private void AddMenuItems(
        ExplorerItem[]  selectedItems,
        MoveSelection   moveSelection,
        ExplorerItem    rootItem)
    {
        DeleteEntity    (selectedItems, rootItem);
        NewEntity       (selectedItems);
        MoveEntityUp    (selectedItems, moveSelection);
        MoveEntityDown  (selectedItems, moveSelection);
    }
    
    private void DeleteEntity(ExplorerItem[] items, ExplorerItem rootItem)
    {
        var isRootItem  = items.Length == 1 && items[0] == rootItem;
        var canDelete   = isRootItem ? items.Length > 1 : items.Length > 0;
        var deleteMenu  = new MenuItem { Header = "Delete entity", IsEnabled = canDelete };
        deleteMenu.InputGesture= new KeyGesture(Key.Delete);
        if (canDelete) {
            deleteMenu.Click += (_, _) => ExplorerCommands.RemoveItems(items, rootItem, grid);
        }
        Items.Add(deleteMenu);
    }
    
    private void NewEntity(ExplorerItem[] items)
    {
        var newMenu  = new MenuItem { Header = "New entity", IsEnabled = items.Length > 0 };
        newMenu.InputGesture= new KeyGesture(Key.N, KeyModifiers.Control);
        if (items.Length > 0) {
            newMenu.Click += (_, _) => ExplorerCommands.CreateItems(items, grid);
        }
        Items.Add(newMenu);
    }
    
    private void MoveEntityUp(ExplorerItem[] items, MoveSelection moveSelection)
    {
        if (moveSelection == null) {
            return;
        }
        var canMove = items.Length > 1 || moveSelection.indexes[0].Last() > 0;
        var newMenu = new MenuItem { Header = "Move up", IsEnabled = canMove };
        newMenu.InputGesture= new KeyGesture(Key.Up, KeyModifiers.Control);
        newMenu.Click += (_, _) => {
            var indexes = ExplorerCommands.MoveItemsUp(items, 1, grid);
            grid.SelectItems(moveSelection, indexes, SelectionView.First);
        };
        Items.Add(newMenu);
    }
    
    private void MoveEntityDown(ExplorerItem[] items, MoveSelection moveSelection)
    {
        if (moveSelection == null) {
            return;
        }
        var canMove = true;
        if (items.Length == 1) {
            var entity  = items.Last().entity;
            var parent  = entity.Parent;
            var index   = parent.GetChildIndex(entity.Id);
            canMove     = index < parent.ChildCount - 1;
        }
        var newMenu = new MenuItem { Header = "Move down", IsEnabled = canMove };
        newMenu.InputGesture= new KeyGesture(Key.Down, KeyModifiers.Control);
        newMenu.Click += (_, _) => {
            var indexes = ExplorerCommands.MoveItemsDown(items, 1, grid);
            grid.SelectItems(moveSelection, indexes, SelectionView.Last);
        };
        Items.Add(newMenu);
    }
}
