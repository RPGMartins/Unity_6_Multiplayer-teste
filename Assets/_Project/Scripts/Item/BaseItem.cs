using System;
using UnityEngine;

[Serializable]
public class BaseItem
{
    [SerializeField] private string itemName;
    [SerializeField] private ItemTypeEnum itemTypeEnum;

    public string ItemName => itemName;
    public ItemTypeEnum ItemTypeEnum => itemTypeEnum;

    public BaseItem(string itemName, ItemTypeEnum itemTypeEnum)
    {
        this.itemName = itemName;
        this.itemTypeEnum = itemTypeEnum;
    }
}

public enum ItemTypeEnum
{
    Weapon,
}
