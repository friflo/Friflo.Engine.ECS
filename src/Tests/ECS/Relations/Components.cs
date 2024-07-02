using Friflo.Engine.ECS;

namespace Tests.ECS.Relations {

internal struct AttackRelation : ILinkRelation
{
    public int    speed;
    public Entity target;
    public Entity GetRelationKey() => target;

    public override string ToString() => target.ToString();
}

internal struct IntRelation : IRelationComponent<int>
{
    public int value;
    public int GetRelationKey() => value;

    public override string ToString() => value.ToString();
}

internal enum InventoryItemType {
    Axe     = 1,
    Gun     = 2,
    Sword   = 3,
    Shield  = 4,
}

/// <summary> <see cref="IRelationComponent{TKey}"/> using an enum as relation key. </summary>
internal struct InventoryItem : IRelationComponent<InventoryItemType>
{
    public InventoryItemType    type;
    public int                  amount;
    public InventoryItemType    GetRelationKey() => type;

    public override string ToString() => type.ToString();
}

}