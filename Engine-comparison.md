
# Engine Comparison


| Friflo Engine                             | Unity                                     | Godot                                     |
| ----------------------------------------- | ----------------------------------------- | ----------------------------------------- |
|   **General properties**                                                                                                          |
| Entity.Id                                 | GameObject.GetInstanceID()                |                                           |
| Entity.Name                               | GameObject.name                           |                                           |
| Entity.Position                           | GameObject.transform.position             |                                           |
| Entity.Rotation                           | GameObject.transform.rotation             |                                           |
| Entity.Scale3                             | GameObject.transform.localScale           |                                           |
| Entity.Store                              | GameObject.scene                          |                                           |
| Entity.Components                         | n/a                                       |                                           |
| Entity.Tags                               | GameObject.tag                            |                                           |
| Entity.Scripts                            | n/a                                       |                                           |
|   **Component methods** (`struct`)                                                                                                |
| Entity.AddComponent<T>()                  | n/a                                       | n/a engine is OOP                         |
| Entity.RemoveComponent<T>()               | n/a                                       |                                           |
| Entity.GetComponent<T>()                  | n/a                                       |                                           |
| Entity.TryGetComponent<T>()               | n/a                                       |                                           |
| Entity.HasComponent<T>()                  | n/a                                       |                                           |
| n/a                                       | n/a                                       |                                           |
|   **`Script` methods** (`class`)          | `MonoBehavior`                                                                        |
| Entity.AddScript<T>()                     | GameObject.AddComponent<>()               |                                           |
| Entity.RemoveScript<T>()                  | UnityEngine.Destroy(object)               |                                           |
| Entity.GetScript<T>()                     | GameObject.GetComponent<>()               |                                           |
| Entity.TryGetScript<T>()                  | GameObject.TryGetComponent<>()            |                                           |
| n/a                                       | GameObject.GetComponents<>()              |                                           |
|   **Tag methods**                                                                                                                 |
| Entity.AddTag<T>()                        | n/a                                       |                                           |
| Entity.AddTags()                          | n/a                                       |                                           |
| Entity.RemoveTag<>()                      | n/a                                       |                                           |
| Entity.RemoveTags()                       | n/a                                       |                                           |
|   **Child properties**                                                                                                            |
| Entity.ChildCount                         | GameObject.transform.childCount           |                                           |
| Entity.ChildEntities                      | n/a                                       |                                           |
| Entity.ChildIds                           | n/a                                       |                                           |
| Entity.Parent                             | GameObject.transform.parent               |                                           |
|   **Child methods**                                                                                                               |
| Entity.AddChild(child)                    | child.parent = parent                     |                                           |
| Entity.RemoveChild(child)                 | child.parent = null                       |                                           |
| Entity.InsertChild()                      | n/a                                       |                                           |
| Entity.DeleteEntity()                     | UnityEngine.Destroy(gameObject)           |                                           |
|   **Events**                                                                                                                      |
| Entity.OnComponentChanged                 | n/a                                       |                                           |
| Entity.OnTagsChanged                      | n/a                                       |                                           |
| Entity.OnScriptChanged                    | n/a                                       |                                           |
| Entity.OnChildEntitiesChanged             | n/a                                       |                                           |
| Entity.AddSignalHandler()                 | n/a                                       |                                           |
| Entity.RemoveSignalHandler()              | n/a                                       |                                           |
| Entity.RemoveSignalHandler()              | n/a                                       |                                           |
|   **Debug properties**                                                                                                            |
| Entity.DebugJSON                          |                                           |                                           |
| Entity.DebugEventHandlers                 |                                           |                                           |
