using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    [SerializeField] private string itemName;
    [SerializeField] private WeaponType weaponType;

    public string ItemName => itemName;
    public WeaponType WeaponType => weaponType;

    public InventoryItem(WeaponType weaponType)
    {
        this.weaponType = weaponType;
    }
}

public enum WeaponType
{
    Flashlight, Pistol, Shotgun, Rifle
}
