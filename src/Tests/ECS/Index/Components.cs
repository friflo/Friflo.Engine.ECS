using System;
using System.Globalization;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Index;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {
    
[ComponentKey("single-attack")]
public struct AttackComponent : ILinkComponent {
    public          Entity      target;
    public          int         data;
    public          Entity      GetIndexedValue()   => target;
    
    public override string      ToString()          => target.ToString();
}

public struct IndexedName : IIndexedComponent<string> {
    public          string      name;
    public          string      GetIndexedValue()   => name;

    public override string      ToString()          => name;
}

public struct IndexedInt : IIndexedComponent<int> {
    public          int         value;
    public          int         GetIndexedValue()   => value;

    public override string      ToString()          => value.ToString();
}

public struct LinkComponent : ILinkComponent {
    public          Entity      entity;
    public          int         data;
    public          Entity      GetIndexedValue()   => entity;

    public override string      ToString()          => entity.Id.ToString();
}

public struct GuidComponent : IIndexedComponent<Guid> {
    public          Guid        guid;
    public          Guid        GetIndexedValue()   => guid;

    public override string      ToString()          => guid.ToString();
}

public struct DateTimeComponent : IIndexedComponent<DateTime> {
    public          DateTime    dateTime;
    public          DateTime    GetIndexedValue()   => dateTime;

    public override string      ToString()          => dateTime.ToString(CultureInfo.InvariantCulture);
}

public enum MyEnum {
    E0 = 0,
    E1 = 1,
    E2 = 2
}

public struct EnumComponent : IIndexedComponent<MyEnum> {
    public          MyEnum      value;
    public          MyEnum      GetIndexedValue()   => value;

    public override string      ToString()          => value.ToString();
}

public struct ComparableEnum : IComparable<ComparableEnum> {
    public          MyEnum      value;

    public override string      ToString()                      => value.ToString();
    public          int         CompareTo(ComparableEnum other) => value - other.value;
    
    
    public static implicit operator ComparableEnum(MyEnum value) => new ComparableEnum { value = value };
}

public struct ComparableEnumComponent : IIndexedComponent<ComparableEnum> {
    public          ComparableEnum  value;
    public          ComparableEnum  GetIndexedValue()   => value;
    
    public override string          ToString()          => value.value.ToString();
}



[CodeCoverageTest]
internal struct IndexedIntRange : IIndexedComponent<int> {
    internal        int         value;
    public          int         GetIndexedValue()   => value;
    
    public override string      ToString()          => value.ToString();
}
    
[ComponentIndex(typeof(RangeIndex<,>))]
internal struct IndexedStringRange : IIndexedComponent<string> {
    internal        string      value;
    public          string      GetIndexedValue()   => value;
    
    public override string      ToString()          => value;
}
}