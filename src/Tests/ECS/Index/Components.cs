using System;
using System.Globalization;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {
    
public struct AttackComponent : ILinkComponent {
    public      Entity  GetIndexedValue() => target;
    public      Entity  target;
}

public struct IndexedName : IIndexedComponent<string> {
    public      string  GetIndexedValue() => name;
    public      string  name;

    public override string ToString() => name;
}

public struct IndexedInt : IIndexedComponent<int> {
    public      int     GetIndexedValue() => value;
    public      int     value;

    public override string ToString() => value.ToString();
}

public struct LinkComponent : ILinkComponent {
    public      Entity  GetIndexedValue() => entity;
    public      Entity  entity;

    public override string ToString() => entity.ToString();
}

public struct GuidComponent : IIndexedComponent<Guid> {
    public      Guid  GetIndexedValue() => guid;
    public      Guid  guid;

    public override string ToString() => guid.ToString();
}

public struct DateTimeComponent : IIndexedComponent<DateTime> {
    public      DateTime  GetIndexedValue() => dateTime;
    public      DateTime  dateTime;

    public override string ToString() => dateTime.ToString(CultureInfo.InvariantCulture);
}

public enum MyEnum {
    E0 = 0,
    E1 = 1,
    E2 = 2
}

public struct EnumComponent : IIndexedComponent<MyEnum> {
    public      MyEnum  GetIndexedValue() => value;
    public      MyEnum  value;

    public override string ToString() => value.ToString();
}

public struct ComparableEnum : IComparable<ComparableEnum> {
    public      MyEnum  value;

    public int CompareTo(ComparableEnum other) {
        return value - other.value;
    }
    
    public static implicit operator ComparableEnum(MyEnum value) => new ComparableEnum { value = value };
}

public struct ComparableEnumComponent : IIndexedComponent<ComparableEnum> {
    public ComparableEnum value;

    public ComparableEnum GetIndexedValue() {
        return value;
    }
    
    public override string ToString() => value.value.ToString();
}



[CodeCoverageTest]
internal struct IndexedIntRange : IIndexedComponent<int> {
    public      int     GetIndexedValue() => value;
    internal    int     value;
    
    public override string ToString() => value.ToString();
}
    
[ComponentIndex(typeof(RangeIndex<>))]
internal struct IndexedStringRange : IIndexedComponent<string> {
    public      string  GetIndexedValue() => value;
    internal    string  value;
    
    public override string ToString() => value;
}
}