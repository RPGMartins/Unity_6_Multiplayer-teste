using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<WeaponItem> weapons = new List<WeaponItem>();
    [SerializeField] private List<BaseItem> items = new List<BaseItem>();

    [SerializeField] private WeaponItem currentWeaponItem;

    public event Action OnInventoryChanged;
    public event Action<WeaponItem> onEquippedSlotChanged;

    private void Start()
    {
        WeaponItem inventoryItemPistol = new WeaponItem("", ItemTypeEnum.Weapon, WeaponTypeEnum.Pistol);
        WeaponItem inventoryItemFlashlight = new WeaponItem("", ItemTypeEnum.Weapon, WeaponTypeEnum.Flashlight);

        AddWeapon(inventoryItemFlashlight);
        AddWeapon(inventoryItemPistol);
        EquipSlotWeapon(0);
    }

    public void AddItemToInventory(BaseItem baseItem)
    {
        if (baseItem is WeaponItem weaponItem)
        {
            AddWeapon(weaponItem);
        }
        else
        {
            AddItem(baseItem);
        }

        OnInventoryChanged?.Invoke();
    }

    private void AddWeapon(WeaponItem weaponItem)
    {
        weapons.Add(weaponItem);
    }

    private void AddItem(BaseItem baseItem)
    {
        items.Add(baseItem);
    }

    public void EquipSlotWeapon(int slot)
    {
        if (slot < 0 || slot >= weapons.Count || weapons[slot] == currentWeaponItem)
        {
            return;
        }

        currentWeaponItem = weapons[slot];
        onEquippedSlotChanged?.Invoke(currentWeaponItem);
    }
}
