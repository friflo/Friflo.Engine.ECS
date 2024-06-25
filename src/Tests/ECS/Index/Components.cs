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