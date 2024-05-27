
# Engine Comparison


| Friflo Engine                             | Unity                                     | Godot                                     |
| ----------------------------------------- | ----------------------------------------- | ----------------------------------------- |
|   **Type**                                                                                                                        |
| • `Entity` **struct**                     | `GameObject` **class**                    | `Node` **class**                          |
|                                                                                                                                   |
|   **General** properties                                                                                                          |
| *Existence: optional*                     | *Existence: always present*               |                                           |
| Entity.Id                                 | GameObject.GetInstanceID()                |                                           |
| Entity.Name                               | GameObject.name                           |                                           |
| Entity.Transform                          | GameObject.transform                      |                                           |
| Entity.Position                           | GameObject.transform.position             |                                           |
| Entity.Rotation                           | GameObject.transform.rotation             |                                           |
| Entity.Scale3                             | GameObject.transform.localScale           |                                           |
| Entity.Store                              | GameObject.scene                          |                                           |
|                                                                                                                                   |
|   **Component** methods                                                                                                           |
| • `IComponent` **struct**                 | • n/a                                     |                                           |
| Entity.Components                         | n/a                                       |                                           |
| Entity.AddComponent<T>()                  | n/a                                       | n/a engine is OOP                         |
| Entity.RemoveComponent<T>()               | n/a                                       |                                           |
| Entity.GetComponent<T>()                  | n/a                                       |                                           |
| Entity.TryGetComponent<T>()               | n/a                                       |                                           |
| Entity.HasComponent<T>()                  | n/a                                       |                                           |
| n/a                                       | n/a                                       |                                           |
|                                                                                                                                   |
|   **Script** methods                                                                                                              |
| • `Script` **class**                      | • `MonoBehavior` **class**                |                                           |
| Entity.Scripts                            | n/a                                       |                                           |
| Entity.AddScript<T>()                     | GameObject.AddComponent<T>()              |                                           |
| Entity.RemoveScript<T>()                  | UnityEngine.Destroy(object)               |                                           |
| Entity.GetScript<T>()                     | GameObject.GetComponent<T>()              |                                           |
| Entity.TryGetScript<T>()                  | GameObject.TryGetComponent<T>()           |                                           |
| n/a                                       | GameObject.GetComponents<T>()             |                                           |
|                                                                                                                                   |
|   **Tag** methods                                                                                                                 |
| • `ITag` **struct**                       | • **`string`**                            |                                           |
| Entity.Tags                               | n/a                                       |                                           |
| n/a                                       | GameObject.tag                            |                                           |
| Entity.Tags.Has<`ITag`>()                 | GameObject.tag == `string`                |                                           |
| Entity.AddTag<T>()                        | n/a                                       |                                           |
| Entity.AddTags()                          | n/a                                       |                                           |
| Entity.RemoveTag<T>()                     | n/a                                       |                                           |
| Entity.RemoveTags()                       | n/a                                       |                                           |
|                                                                                                                                   |
|   **Child** properties                                                                                                            |
| • `Entity` **struct**                     | • `GameObject` **class**                  |                                           |
| Entity.ChildCount                         | GameObject.transform.childCount           |                                           |
| Entity.ChildEntities                      | n/a                                       |                                           |
| Entity.ChildIds                           | n/a                                       |                                           |
| Entity.Parent                             | GameObject.transform.parent               |                                           |
|                                                                                                                                   |
|   **Child** methods                                                                                                               |
| Entity.AddChild(child)                    | child.parent = parent                     |                                           |
| Entity.RemoveChild(child)                 | child.parent = null                       |                                           |
| Entity.InsertChild()                      | n/a                                       |                                           |
| Entity.DeleteEntity()                     | UnityEngine.Destroy(gameObject)           |                                           |
|                                                                                                                                   |
|   **Events** *(build-in)*                                                                                                         |
| Entity.OnComponentChanged                 | n/a                                       |                                           |
| Entity.OnTagsChanged                      | n/a                                       |                                           |
| Entity.OnScriptChanged                    | n/a                                       |                                           |
| Entity.OnChildEntitiesChanged             | n/a                                       |                                           |
|                                                                                                                                   |
|   **Signals** *(custom events)*                                                                                                   |
| Entity.AddSignalHandler()                 | n/a                                       |                                           |
| Entity.RemoveSignalHandler()              | n/a                                       |                                           |
| Entity.EmitSignal()                       | n/a                                       |                                           |
|                                                                                                                                   |
|   **Debug** properties                                                                                                            |
| Entity.DebugEventHandlers                 |                                           |                                           |
| Entity.DebugJSON                          |                                           |                                           |
