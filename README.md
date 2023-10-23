# Engine architecture


| Topic             | Project                   | Link                                                                                                      |
| ----------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| Graphics API      | OpenGL ES 3.2 /           | [Open Graphics Library for Embedded Systems – Wikipedia](https://de.wikipedia.org/wiki/Open_Graphics_Library_for_Embedded_Systems)
|                   | Silk.NET                  | [Silk.NET](https://github.com/dotnet/Silk.NET)
| Physics           | bepuphysics2 (not sure)   | [Pure C# 3D real time physics simulation library, now with a higher version](https://github.com/bepu/bepuphysics2)
|                   |                           | [physics-engine github topics/ C#](https://github.com/topics/physics-engine?l=c%23)
| Data structure    | ECS                       | [ECS - FAQ](https://github.com/SanderMertens/ecs-faq)
|                   | SIMD                      | [Get Started with the Unity* Entity Component System (ECS), C# Job...](https://www.intel.com/content/www/us/en/developer/articles/guide/get-started-with-the-unity-entity-component-system-ecs-c-sharp-job-system-and-burst-compiler.html)
|                   |                           | [Single Instruction, Multiple Data (SIMD) in .NET](https://antao-almada.medium.com/single-instruction-multiple-data-simd-in-net-393b8cf9a90)
| Storage           | Fliox.Hub JSON / SQLite   | [Database providers](https://github.com/friflo/Friflo.Json.Fliox#-database-providers)
| Netcode           | Fliox.Hub Protocol        | [Hub Protocol](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Json/Fliox.Hub/Protocol/README.md)
|                   | Client                    | [FlioxClient](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Json/Fliox.Hub/Client/README.md)
|                   | Server                    | [FlioxHub](https://github.com/friflo/Friflo.Json.Fliox/blob/main/Json/Fliox.Hub/Host/README.md)
| Scripting         | C#                        | As the whole engine is implement in .NET calling its API has no marshalling overhead
| Editor UI         | Avalonia UI               | [Avalonia Play](https://play.avaloniaui.net/)
|                   |                           | [Avalonia - Get Started](https://docs.avaloniaui.net/docs/next/welcome)
| Editor Reload     | AssemblyLoadContext       | C# using AssemblyLoadContext to edit / unload / reload editor workflow loop
|                   |                           | [Example](https://github.com/dotnet/samples/blob/main/core/tutorials/Unloading/Host/Program.cs)
|                   |                           | [How to use and debug assembly unloadability in .NET](https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability)


<br/><br/>



## Graphics API

*Alternatives*
- [OpenTK](https://github.com/opentk/opentk)
- [Veldrid](https://github.com/veldrid/veldrid)

<br/><br/>



## Physics

*Alternatives*

<br/><br/>



## Editor UI

[Avalonia](https://docs.avaloniaui.net/docs/next/welcome)
supported platform: Windows, macOS, Linux, iOS, Android, WebAssembly & Tizen.  
Is uses the [Skia rendering engine](https://docs.avaloniaui.net/docs/getting-started/programming-with-avalonia/graphics-and-animations)
by default.  
This enables UI rendering is exactly the same on all platforms with a single codebase.

*Alternatives*
- [Uno Platform](https://github.com/unoplatform/uno)
- [Uno Platform: Your Apps Everywhere - Martin Zikmund - NDC London 2023 - YouTube](https://www.youtube.com/watch?v=BTgp9VyZSs8)

<br/><br/>



## Scripting

<br/><br/>


## `GameEntityStore` / `GameDatabase`

Both `GameEntityStore` and `GameDatabase` are used to store game entities.  
The `GameEntityStore` store `GameEntity`'s at runtime.  
The `GameDatabase` store serialized `GameEntity`'s as `DatabaseEntity`' in databases or files and transfer them via a network.  

Using the `GameEntityStore` / `GameDatabase` enables instant synchronization of a game scene between multiple creators.  
In this workflow the game scene is stored in a shared database without using a version control system like `Git`.  

The common approach to store game scenes in scene files is also available.  
It is the preferred use case when sharing demos or assets via a GitHub project or a file bundle.


### `GameEntityStore`

The `GameEntityStore` is used almost without exception when scripting game logic / behavior with **C#**.  
It stores game entities at runtime by using an ECS data structure.  
Its focus is simplicity and performance.  

Scripting is mainly driven by the API of `GameEntity`'s stored in a `GameEntityStore`.  
The performance and efficiency is implemented by using ECS as data structure.

A `GameEntityStore` stores game entities in a hierarchy - a tree graph to build a game scene.  
Game entities in this hierarchy can be declared as sub scene to persist them as separate scene files on disk.  
The use case of sub scenes is to enhance the scene structure and minimize merge conflicts when used by multiple creators.


### `GameDatabase`

The `GameDatabase` is used to convert `GameEntity` to `DatabaseEntity`s or vise versa so that they can be stored in 
scene files or in a database.




## Data structure

The engine uses **ECS** as its fundamental data structure.  
The strength of **ECS** is enabling high memory locality and reduced memory usage by storing entity data in linear memory.

Many stages in the pipeline processing entity data benefits from this approach.

```mermaid
    graph LR;
        Read(Read Input);

        UpdateEntities{{1. Update entity<br/>Game logic}};

        Simulation{{2. Physics<br/>simulation}};

        ProcessCollision{{3. Process<br/>collisions}};

        Rendering{{4. Render<br/>entities}};

        EntityState(<b>Entity state</b><br/>Position, Rotation<br/>linear & radial Velocity<br/>linear & radial Acceleration);

        Assets(Textures & vertices);


        Read            -->|Input|UpdateEntities;
        UpdateEntities  -->|updated<br/>Physics|Simulation;
        UpdateEntities  -->|updated<br/>Game state|EntityState;
        Simulation      -->|Collisions|ProcessCollision
        Simulation      -->|updated<br/>Positions<br/>Rotations<br/>Velocities|EntityState;
        ProcessCollision-->|update<br/>Game state|EntityState;
        EntityState     -.->Simulation;
        EntityState     -.->UpdateEntities;
        EntityState     -.->|Positions|Rendering;
        Assets          -.->Rendering;
```



Features of an `EntityStore`
- Store a map (container) of entities in linear memory.
- Organize entities in a tree structure starting with a single root entity.
- Store the components (e.g. `Position`) of entities with the same `Archetype` in linear memory.  
  Basically the **E** & **C** of an **ECS** architecture.

Types:
- `EntityStore`     - the storage for all entities and their components.
- `GameEntity`      - each instances contains the following properties
    - `id`          - type: `int` / id > 0
    - `components`
        - **Archetype** components stored in `Archetype`'s.  
          These are value types (`struct`) having only data and **no** behavior (methods).
        - **Behavior** components.  
          Behavior components are reference types (`class`) and have behavior (methods).
    - `tags`        - list of tags assigned to an entity. Tags have no data.
    - `children`    - contains and array of child entity `id`'s.
- `Archetype`       - contains all entities with the same set of **struct** `IComponent` types.  
The **struct** components of an `Archetype` are stored linear in memory to improve memory locality.  
Each component is indexed from 0, ... , N.  
Its property `EntityIds` stores the entity `id`'s each component is owned by.


Serialized entity example

```javascript
{
    "id": 11,
    "components": {                         // can be null
        "name": "Root",                     // Archetype component
        "pos": { x: 1, y: 2, x: 3},         // Archetype component
        "rot": { x: 0, y: 0, x: 0, w: 0 },  // Archetype component
        "my1": { a: 1 }                     // Behavior component
    },
    "tags":["PlayerTag"],                   // can be null
    "children": [1,2,3]                     // can be null
}
```

Note:  
Both component types are serialized into the same `components` array.  
The engines uses the registered **`IComponent`** or **`Behavior`** type for serialization.

This enables reading already serialized data after refactoring a **`Behavior`** to a **`struct`** `IComponent` or vice versa.


### Entity serialization model

Entities are loaded using a `GameClient`

```csharp
public sealed class DatabaseEntity
{
    public  long            id;         // pid - permanent id
    public  List<long>      children;   // can be null
    public  JsonValue       components; // can be null
    public  List<string>    tags;       // can be null
    public  string          preFab;     // can be null
    public  string          nodeRef;    // can be null
}

public class GameClient : FlioxClient
{
    public  readonly    EntitySet <long, DatabaseEntity>   entities;
    
    public GameClient(FlioxHub hub, string dbName = null) : base (hub, dbName) { }
}
```

Remarks:

- Each entity has a unique id.

- Entities of a scene can be store in various ways:
  - Store all entities in a single JSON file. The order of entities is preserved to minimize merge conflicts.
  - Store each entity in an individual file using its id as file name.
  - Store all entities in a relational database like SQLite using the entity id as the primary key.

- Entity `id`'s used in a scene are stable (permanent). So references to them are stable too.

- Each entity must have only one parent so it must be included in only one `DatabaseEntity.children`.

- When creating new entities in a scene the engine creates random `id`'s by default using `PidType.RandomPids`.

  Using random id's avoid merge conflicts when multiples users make changes to the same scene file / database.  
  The probability generating the same id by two different users is:

  p = 1 - exp(-r^2 / (2 * N))
  
  r:  number of new entities added by a user to an existing scene (not the number of all entities)  
  N:  number of possible values = int.MaxValue = 2147483647

  E.g Adding 1000 entities by two different users to the same scene.  
  p = 1 - exp(-1000^2 / (2 * 2147483647)) = 0.000232 = 0.0232 %
  
  See: https://en.wikipedia.org/wiki/Birthday_problem

### Loading entities

Entities are loaded in batches of 10.000 entities using the `GameClient`.  
If a batch has finished loading `DatabaseEntity`'s they are than added to the `EntityStore`
by calling `GameDatabase.LoadEntity()` for each `DatabaseEntity`.

The entity tree is build by utilizing the field `children` of a `DatabaseEntity`.  
In case ids in `children` are inconsistent the errors can be ignored or cause a loading error.

Possible inconsistencies:
- Same entity id in `children` is used in multiple records.
- Entities are referencing each other resulting in a cyclic dependency.
