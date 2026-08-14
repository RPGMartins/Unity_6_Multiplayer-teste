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

        TryAddWeapon(inventoryItemFlashlight);
        TryAddWeapon(inventoryItemPistol);
        EquipSlotWeapon(0);
    }

    public bool TryAddItemToInventory(BaseItem baseItem)
    {
        bool addedItem = false;

        if (baseItem is WeaponItem weaponItem)
        {
            addedItem = TryAddWeapon(weaponItem);
        }
        else
        {
            addedItem = TryAddItem(baseItem);
        }

        if (addedItem == false)
        {
            return false;
        }

        OnInventoryChanged?.Invoke();

        return true;
    }

    private bool TryAddWeapon(WeaponItem weaponItem)
    {
        weapons.Add(weaponItem);

        return true;
    }

    private bool TryAddItem(BaseItem baseItem)
    {
        items.Add(baseItem);

        return true;
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
