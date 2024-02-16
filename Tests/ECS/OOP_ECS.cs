using System;
using System.Collections.Generic;
using System.Linq;
using Friflo.Engine.ECS;
using NUnit.Framework;

// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedVariable
namespace Tests.ECS;

public static class ExampleECS
{

// No base class Animal in ECS
struct Dog : ITag { }
struct Cat : ITag { }

[Test]
public static void ECS()
{
    var store = new EntityStore();
    
    Archetype dogType = store.GetArchetype(Tags.Get<Dog>());
    Archetype catType = store.GetArchetype(Tags.Get<Cat>());
    Console.WriteLine(dogType.Name);            // [#Dog]
    
    dogType.CreateEntity();
    catType.CreateEntity();
    
    var dogs = store.Query().AnyTags(Tags.Get<Dog>());
    var all  = store.Query().AnyTags(Tags.Get<Dog, Cat>());
    
    Console.WriteLine($"dogs: {dogs.Count}");   // > dogs: 1
    Console.WriteLine($"all: {all.Count}");     // > all: 2
}


}


public static class ExampleOOP
{


class Animal { }
class Dog : Animal { }
class Cat : Animal { }

[Test]
public static void OOP()
{
    var animals = new List<Animal>();
    
    var dogType = typeof(Dog);
    var catType = typeof(Cat);
    Console.WriteLine(dogType.Name);            // > Dog
    
    animals.Add(new Dog());
    animals.Add(new Cat());
    
    var dogs = animals.Where(animal => animal is Dog);
    var all  = animals.Where(animal => animal is Dog or Cat);
    
    Console.WriteLine($"dogs: {dogs.Count()}"); // > dogs: 1
    Console.WriteLine($"all: {all.Count()}");   // > all: 2
}


}    

