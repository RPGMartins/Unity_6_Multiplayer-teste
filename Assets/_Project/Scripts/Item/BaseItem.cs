using System;
using UnityEngine;

[Serializable]
public class BaseItem
{
    [SerializeField] private string itemID;
    [SerializeField] private ItemTypeEnum itemTypeEnum;

    public string ItemID => itemID;
    public ItemTypeEnum ItemTypeEnum => itemTypeEnum;

    public BaseItem(string itemID, ItemTypeEnum itemTypeEnum)
    {
        this.itemID = itemID;
        this.itemTypeEnum = itemTypeEnum;
    }
}

public enum ItemTypeEnum
{
    Weapon,
}
