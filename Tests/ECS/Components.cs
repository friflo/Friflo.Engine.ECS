using System;
using System.Collections.Generic;
using System.Numerics;
using Friflo.Engine.ECS;
using static NUnit.Framework.Assert;

#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value

// ReSharper disable RedundantTypeDeclarationBody
namespace Tests.ECS;

// ------------------------------------------------ components
[CodeCoverageTest]
[ComponentKey("my1")]
public struct MyComponent1 : IComponent { public int a; }

[ComponentKey("my2")]
public struct MyComponent2 : IComponent { public int b; }

public struct NonBlittableArray         : IComponent { internal int[]                   array;  }
public struct NonBlittableList          : IComponent { internal List<int>               list;   }
public struct NonBlittableDictionary    : IComponent { internal Dictionary<int, int>    map;    }

public struct BlittableDatetime         : IComponent { public DateTime      dateTime;    }
public struct BlittableGuid             : IComponent { public Guid          guid;        }
public struct BlittableBigInteger       : IComponent { public BigInteger    bigInteger;  }
// public struct BlittableUri           : IComponent { public Uri           uri;         } todo requires fix in Fliox.Mapper



[CodeCoverageTest]
[ComponentKey("byte")]
public struct ByteComponent : IComponent { public byte b; }

public struct LongComponent : IComponent { public long b; }

/// <summary>Example shows an extension class to enable component access using less code.</summary>
public static class MyEntityExtensions
{
    public static ref MyComponent1 MyComponent1(this Entity entity) => ref entity.GetComponent<MyComponent1>();
    public static ref MyComponent2 MyComponent2(this Entity entity) => ref entity.GetComponent<MyComponent2>();
}

// test missing [StructComponent()] attribute
struct MyInvalidComponent : IComponent { public int b; }


// ------------------------------------------------ tags
[TagName("test-tag")]
public struct TestTag  : ITag { }

[CodeCoverageTest]
[TagName("test-tag2")]
public struct TestTag2 : ITag { }

// Intentionally without [Tag("test-tag3")] attribute for testing
public struct TestTag3 : ITag { }

public struct TestTag4 : ITag { }

public struct TestTag5 : ITag { }


// ------------------------------------------------ scripts
[CodeCoverageTest]
[ComponentKey("script1")]
public class TestScript1    : Script { public   int     val1; }

[ComponentKey("script2")]
class TestScript2           : Script { public   int     val2; }

[ComponentKey("script3")]
class TestScript3           : Script { public   int     val3; }

class NonBlittableScript    : Script { internal int[]   array; }

[ComponentKey("test")]
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

/// <summary> Used only to cover <see cref="TagType.NewTagIndex"/>,
/// <see cref="StructUtils.NewStructIndex"/> and <see cref="ScriptType.NewScriptIndex"/></summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class CodeCoverageTestAttribute : Attribute { }