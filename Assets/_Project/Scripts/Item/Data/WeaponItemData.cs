using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "Data/Weapon", order = 1)]
public class WeaponItemData : ItemData
{
    [SerializeField] private WeaponTypeEnum weaponTypeEnum;

    public WeaponTypeEnum WeaponTypeEnum => weaponTypeEnum;

    public override BaseItem ToBaseItem()
    {
        WeaponItem weaponItem = new WeaponItem(ItemID, ItemTypeEnum, weaponTypeEnum);

        return weaponItem;
    }
}
