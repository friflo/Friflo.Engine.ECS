using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor;

using System;

public static class Program
{
    public static void Main(string[] args) {
        Console.WriteLine("Hello, World!");
        var store   = new GameEntityStore();
        var entity  = store.CreateEntity();
        entity.AddComponent(new Position());
        Console.WriteLine($"entity: {entity}");
    }
}

