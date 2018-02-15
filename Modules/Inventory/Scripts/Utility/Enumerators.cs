using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enumerators : MonoBehaviour
{
}

public enum damageType
{
    Blunt,
    Sharp,
    Pierce
}

public enum CombatState
{
    OutOfCombat,
    InCombat,
    Hiding
}


public enum AttackHeight
{
    Down,
    Normal
}

public enum HoverType
{
    Inventory,
    Container,
    Shop,
    Bag,
    Recipe,
    DragBar,
    HotBar,
    Saves,
    PreventHotBarScrolling,
    InteractionWheel,
    Nothing
}

public enum SaveableItemType
{
    Static,
    Harvestable,
    DroppedItem,
    Container
}

[System.Serializable]
public class DamageableProperties
{
    [SerializeField]
    public double damageAmmount;

    [SerializeField]
    public damageType typeOfDamage;

    public DamageableProperties()
    {

    }

    public DamageableProperties(double _dmgAmnt, damageType _type)
    {
        damageAmmount = _dmgAmnt;
        typeOfDamage = _type;
    }
}
