using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Friflo.Fliox.Editor.UI.Explorer;

// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable ReplaceSliceWithRangeIndexer
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
            var indexes         = selection.SelectedIndexes;
            var rootItem        = explorer.RootItem;
            var moveSelection   = MoveSelection.Create(indexes);
            AddMenuItems(items, moveSelection, rootItem);
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
        IReadOnlyList<ExplorerItem> selectedItems,
        MoveSelection               moveSelection,
        ExplorerItem                rootItem)
    {
        var items       = selectedItems.ToArray();
        DeleteEntity    (items, rootItem);
        NewEntity       (items);
        MoveEntityUp    (items, moveSelection);
        MoveEntityDown  (items, moveSelection);
    }
    
    private void DeleteEntity(ExplorerItem[] items, ExplorerItem rootItem)
    {
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
    
    private void MoveEntityUp(ExplorerItem[] items, MoveSelection moveSelection)
    {
        if (moveSelection == null) {
            return;
        }
        var canMove = items.Length > 1 || moveSelection.indexes[0].Last() > 0;
        var newMenu = new MenuItem { Header = "Move up", IsEnabled = canMove };
        newMenu.Click += (_, _) => {
            var indexes = ExplorerCommands.MoveItemsUp(items, 1);
            SelectItems(moveSelection, indexes);
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
        newMenu.Click += (_, _) => {
            var indexes = ExplorerCommands.MoveItemsDown(items, 1);
            SelectItems(moveSelection, indexes);
        };
        Items.Add(newMenu);
    }
    
    private void SelectItems(MoveSelection moveSelection, int[] indexes)
    {
        var parent      = moveSelection.parent;
        var selection   = explorer.DragDrop.RowSelection!;
        selection.BeginBatchUpdate();
        foreach (var index in indexes) {
            var child = parent.Append(index);
            selection.Select(child);
        }
        selection.EndBatchUpdate();
    }
}

internal class MoveSelection
{
    internal            IndexPath                   parent;
    internal readonly   IReadOnlyList<IndexPath>    indexes;

    public   override   string      ToString() => parent.ToString();

    private MoveSelection(in IndexPath parent, IReadOnlyList<IndexPath> indexes) {
        this.parent     = parent;
        this.indexes    = indexes;
    }
    
    internal static MoveSelection Create(IReadOnlyList<IndexPath> indexes)
    {
        if (indexes.Count == 0) {
            return null;
        }
        var first   = indexes[0];
        if (first == new IndexPath(0)) {
            return null;
        }
        var parent  = indexes[0].Slice(0, first.Count - 1);
        for (int n = 1; n < indexes.Count; n++)
        {
            var index = indexes[n];
            if(!parent.IsParentOf(index)) {
                return null;
            }
        }
        return new MoveSelection(parent, indexes);
    }
}
