using System;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [SerializeField] private string itemID;
    [SerializeField] private ItemTypeEnum itemTypeEnum;

    public string ItemID => itemID;
    public ItemTypeEnum ItemTypeEnum => itemTypeEnum;

    public abstract BaseItem ToBaseItem();
}
