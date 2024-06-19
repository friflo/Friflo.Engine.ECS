using System;
using System.Collections.Generic;
using System.Numerics;
using Friflo.Engine.ECS;
using static NUnit.Framework.Assert;

#pragma warning disable CS0649 // Field '...' is never assigned to, and will always have its default value

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable RedundantTypeDeclarationBody
namespace Tests.ECS {

// ------------------------------------------------ components
[CodeCoverageTest]
[ComponentKey("my1")]
[ComponentSymbol("M1")]
public struct MyComponent1 : IComponent {
    public          int     a;
    public override string  ToString() => a.ToString();
}

internal class CycleClass  { internal CycleClass    cycle;  }

// two classes with indirect type cycle
internal class CycleClass1 { internal CycleClass2   cycle2; }
internal class CycleClass2 { internal CycleClass1   cycle1; }

[ComponentKey("my2")]
[ComponentSymbol("M2 too long")]
public struct MyComponent2 : IComponent { public int b; }

[ComponentKey("my3")]
[ComponentSymbol(" M3", "invalid")]
public struct MyComponent3 : IComponent { public int b; }

[ComponentKey("my4")]
[ComponentSymbol("", "invalid1,invalid2,invalid3")]
public struct MyComponent4 : IComponent { public int b; }

[ComponentKey("my5")]
public struct MyComponent5 : IComponent { public int b; }

[ComponentKey("my6")]
public struct MyComponent6 : IComponent { public int b; }

[ComponentKey("my7")]
public struct MyComponent7 : IComponent { public int b; }

public struct NonBlittableArray         : IComponent { internal int[]                   array;  }
public struct NonBlittableList          : IComponent { internal List<int>               list;   }
public struct NonBlittableDictionary    : IComponent { internal Dictionary<int, int>    map;    }
public struct NonBlittableCycle         : IComponent { internal CycleClass              cycle;  }
public struct NonBlittableCycle2        : IComponent { internal CycleClass1             cycle1; }

public struct BlittableDatetime         : IComponent { public DateTime      dateTime;    }
public struct BlittableGuid             : IComponent { public Guid          guid;        }
public struct BlittableBigInteger       : IComponent { public BigInteger    bigInteger;  }
public struct BlittableUri              : IComponent { public Uri           uri;         } // throws exception when serialized

[ComponentKey(null)]
public struct NonSerializedComponent    : IComponent { public int           value;  }

[CodeCoverageTest]
[ComponentKey("byte")]
public struct ByteComponent     : IComponent { public byte  b; }
public struct ShortComponent    : IComponent { public short b; }
public struct IntComponent      : IComponent { public int   b; }
public struct LongComponent     : IComponent { public long  b; }

// see [Integral numeric types - C# reference - C# | Microsoft Learn] https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
public struct NonClsTypes : IComponent
{
    public  sbyte   int8;
    public  ushort  uint16;
    public  uint    uint32;
    public  ulong   uint64;
    
    public  sbyte?  int8Null;
    public  ushort? uint16Null;
    public  uint?   uint32Null;
    public  ulong?  uint64Null;
}

public struct Component20       : IComponent
{
    public int  val1;
    public int  val2;
    public int  val3;
    public int  val4;
    public int  val5;
}

public struct Component16       : IComponent
{
    public long  val1;
    public long  val2;
}

public struct Component32       : IComponent
{
    public long  val1;
    public long  val2;
    public long  val3;
    public long  val4;
}

public struct Component64       : IComponent
{
    public long  val1;
    public long  val2;
    public long  val3;
    public long  val4;
    public long  val5;
    public long  val6;
    public long  val7;
    public long  val8;
}

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
public class TestScript2    : Script { public   int     val2; }

[ComponentKey("script3")]
class TestScript3           : Script { public   int     val3; }

[ComponentKey("script4")]
class TestScript4           : Script { public   int     val4; }

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

/// <summary> Used only to cover <see cref="SchemaTypeUtils.GetStructIndex"/>
/// Deprecated methods
/// TagType.NewTagIndex()
/// ScriptType.NewScriptIndex()
/// </summary> 
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class CodeCoverageTestAttribute : Attribute { }

}

// -------------- Cover duplicate component keys & tag names
namespace Tests.Duplicates
{
    // Cover: warning: Duplicate component key  for a component
    [ComponentKey("my7")]
    internal struct DupMyComponent7 : IComponent { }

    // Cover: warning: Duplicate component key  for a script
    [ComponentKey("script4")]
    internal class DupScript4 : Script { }
    
    // Cover: warning: Duplicate tag name for a tag
    [TagName("TestTag5")]
    internal struct DupTag5 : ITag { }
}