using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Friflo.Fliox.Editor.UI.Explorer.Lab;
using Friflo.Fliox.Engine.ECS;
using Friflo.Fliox.Engine.ECS.Collections;

namespace Friflo.Fliox.Editor.UI.Explorer;

// see: https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/samples/TreeDataGridDemo/ViewModels/DragDropPageViewModel.cs
internal class ExplorerViewModel
{
    private static ITreeDataGridSource<ExplorerItem> CreateExplorerItemSource()
    {
        // var store       = ExplorerTree.TestStore;
        var store       = CreateDefaultStore();
        var rootEntity  = store.StoreRoot;
        var tree        = new ExplorerTree(rootEntity, "DefaultStore");
        var items       = new [] { tree.RootItem } as IEnumerable<ExplorerItem>;
        // items           = Array.Empty<ExplorerItem>();
        
        var source  = new HierarchicalTreeDataGridSource<ExplorerItem>(items);
        
        var nameCol = new TextColumn<ExplorerItem, string>("name", item => item.Name, (item, value) => item.Name = value, GridLength.Star);
        source.Columns.Add(new HierarchicalExpanderColumn<ExplorerItem>(nameCol, item => item));
        
        var idCol   = new TextColumn<ExplorerItem, int>   ("id",   item => item.Id,   GridLength.Auto);
        source.Columns.Add(idCol);
        
        var flagCol = new CheckBoxColumn<ExplorerItem>    ("flag", item => item.flag, (item, value) => item.flag = value);
        source.Columns.Add(flagCol);

        source.RowSelection!.SingleSelect = false;
        return source;
    }
    
    private static GameEntityStore CreateDefaultStore()
    {
        var store   = new GameEntityStore(PidType.UsePidAsId);
        var root    = store.CreateEntity();
        root.AddComponent(new EntityName("root - default"));
        store.SetStoreRoot(root);
        return store;
    }
    
    internal ITreeDataGridSource<ExplorerItem> ExplorerItemSource => CreateExplorerItemSource();
    
    // --------------------------------------- test data source ---------------------------------------
    internal static readonly GameEntityStore TestStore = CreateTestStore(); 
    
    private static GameEntityStore CreateTestStore()
    {
        var store   = new GameEntityStore();
        var root =    CreateEntity(store, 1, "root");
        root.AddChild(CreateEntity(store, 2, "child 2"));
        root.AddChild(CreateEntity(store, 3, "child 3"));
        root.AddChild(CreateEntity(store, 4, "child 4"));
        root.AddChild(CreateEntity(store, 5, "child 5"));
        root.AddChild(CreateEntity(store, 6, "child 6"));
        store.SetStoreRoot(root);
        return store;
    }
    
    private static GameEntity CreateEntity(GameEntityStore store, int id, string name)
    {
        var entity = store.CreateEntity(id);
        entity.AddComponent(new EntityName(name));
        return entity;
    }
    
    // --------------------------------------- example data source ---------------------------------------
    /// Example showing the creation of a simple data source.
    private static ITreeDataGridSource<DragDropItem> CreateDragDropSource()
    {
        var data    = DragDropItem.Root;
        var source  = new HierarchicalTreeDataGridSource<DragDropItem>(data);
        
        source.Columns.Add(new HierarchicalExpanderColumn<DragDropItem>(
            new TextColumn<DragDropItem, string>("Name", x => x.Name, GridLength.Star), x => x.children));
        
        source.Columns.Add(
            new CheckBoxColumn<DragDropItem>("Flag", item => item.flag, (item, value) => item.flag = value));

        source.RowSelection!.SingleSelect = false;
        return source;
    }

    internal ITreeDataGridSource<DragDropItem> DragDropSource => CreateDragDropSource();
}

