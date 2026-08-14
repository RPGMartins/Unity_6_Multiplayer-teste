using System;
using UnityEngine;

[Serializable]
public class WeaponItem : BaseItem
{
    [SerializeField] private WeaponTypeEnum weaponTypeEnum;

    public WeaponTypeEnum WeaponTypeEnum => weaponTypeEnum;

    public WeaponItem(string itemID, ItemTypeEnum itemTypeEnum, WeaponTypeEnum weaponTypeEnum) : base(itemID, itemTypeEnum)
    {
        this.weaponTypeEnum = weaponTypeEnum;
    }
}

public enum WeaponTypeEnum
{
    Flashlight, Pistol, Shotgun, Rifle
}
