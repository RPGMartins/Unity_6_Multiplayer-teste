using System;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [SerializeField] private String itemName;
    [SerializeField] private ItemTypeEnum itemTypeEnum;

    public String ItemName => itemName;
    public ItemTypeEnum ItemTypeEnum => itemTypeEnum;

    public abstract BaseItem ToBaseItem();
}
