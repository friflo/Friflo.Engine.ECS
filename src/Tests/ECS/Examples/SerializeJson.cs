using System;
using System.IO;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Serialize;
using Friflo.Json.Fliox;
// ReSharper disable ArrangeTypeMemberModifiers

// ReSharper disable NotAccessedField.Local
// ReSharper disable once CheckNamespace
namespace Tests.Examples
{

internal static class SerializeJson {
    
[ComponentKey("data")]  // use "data" as component key in JSON
struct DataComponent : IComponent
{
    [Ignore]            // field is ignored in JSON
    public int          temp;
    
    [Serialize("n")]    // use "n" as field key in JSON 
    public string       name;
}

[NUnit.Framework.TestAttribute]
public static void JsonSerialization()
{
    var store = new EntityStore();
    store.CreateEntity(new EntityName("hello JSON"));
    store.CreateEntity(new Position(1, 2, 3));
    store.CreateEntity(new DataComponent{ temp = 42, name = "foo" });

    // --- Write store entities as JSON array
    var serializer = new EntitySerializer();
    var writeStream = new FileStream("entity-store.json", FileMode.Create);
    serializer.WriteStore(store, writeStream);
    writeStream.Close();

    // --- Read JSON array into new store
    var targetStore = new EntityStore();
    serializer.ReadIntoStore(targetStore, new FileStream("entity-store.json", FileMode.Open));

    Console.WriteLine($"entities: {targetStore.Count}"); // > entities: 3
}

}
}
