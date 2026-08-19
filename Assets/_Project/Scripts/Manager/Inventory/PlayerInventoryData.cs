using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class PlayerInventoryData
{
    [SerializeField] private FixedString64Bytes authenticateID;
    [SerializeField] private List<WeaponItem> weaponList = new List<WeaponItem>();

    public FixedString64Bytes AuthenticateID => authenticateID;

    public PlayerInventoryData(FixedString64Bytes authenticateID)
    {
        this.authenticateID = authenticateID;
    }

    public void AddWeapon(WeaponItem weaponItem)
    {
        weaponList.Add(weaponItem);
    }

    public void RemoveWeapon(WeaponItem weaponItem)
    {
        weaponList.Remove(weaponItem);
    }

    public BaseItem GetWeaponBySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponList.Count)
        {
            return null;
        }

        return weaponList[slotIndex];
    }
}
