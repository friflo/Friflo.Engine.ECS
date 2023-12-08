using System;
using System.Collections.Generic;
using System.Numerics;
using Friflo.Fliox.Engine.ECS;
using static NUnit.Framework.Assert;

#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value

namespace Tests.ECS;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class CodeCoverageTestAttribute : Attribute { }

// ------------------------------------------------ components
[CodeCoverageTest]
[Component("my1")]
public struct MyComponent1 : IComponent { public int a; }

[Component("my2")]
public struct MyComponent2 : IComponent { public int b; }

public struct NonBlittableArray         : IComponent { internal int[]                   array;  }
public struct NonBlittableList          : IComponent { internal List<int>               list;   }
public struct NonBlittableDictionary    : IComponent { internal Dictionary<int, int>    map;    }

public struct BlittableDatetime         : IComponent { public DateTime      dateTime;    }
public struct BlittableGuid             : IComponent { public Guid          guid;        }
public struct BlittableBigInteger       : IComponent { public BigInteger    bigInteger;  }
// public struct BlittableUri           : IComponent { public Uri           uri;         } todo requires fix in Fliox.Mapper



[CodeCoverageTest]
[Component("byte")]
public struct ByteComponent : IComponent { public byte b; }

/// <summary>Example shows an extension class to enable component access using less code.</summary>
public static class EntityExtensions
{
    public static ref MyComponent1 MyComponent1(this Entity entity) => ref entity.GetComponent<MyComponent1>();
    public static ref MyComponent2 MyComponent2(this Entity entity) => ref entity.GetComponent<MyComponent2>();
}

// test missing [StructComponent()] attribute
struct MyInvalidComponent : IComponent { public int b; }


// ------------------------------------------------ tags
public struct TestTag  : IEntityTag { }

public struct TestTag2 : IEntityTag { }

public struct TestTag3 : IEntityTag { }


// ------------------------------------------------ scripts
[CodeCoverageTest]
[Script("script1")]
public class TestScript1    : Script { public   int     val1; }

[Script("script2")]
class TestScript2           : Script { public   int     val2; }

[Script("script3")]
class TestScript3           : Script { public   int     val3; }

class NonBlittableScript    : Script { internal int[]   array; }

[Script("test")]
class TestComponent : Script
{
    private int                      health;     // is serialized
    
    public override  void Update() {
        health = 4;
        Entity.GetComponent<MyComponent1>().a = 5 + health;
        Entity.Position.x += 1;
        AreEqual(2f, Entity.Position.x);
    }
}
