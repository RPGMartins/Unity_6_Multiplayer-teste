using System;
using Unity.Collections;
using UnityEngine;

public class PlayerLocalInventoryController : MonoBehaviour
{
    public Action onLocalInventoryChanged;
    public event Action<WeaponItem> onEquippedSlotChanged;

    private FixedString64Bytes authenticateID;

    public void Initialize(FixedString64Bytes authenticatrID)
    {
        this.authenticateID = authenticatrID;
    }

    public void EquipSlotWeapon(int slot)
    {
        BaseItem baseItem = InventoryManager.Instance.GetPlayerItemBySlot(authenticateID, slot);

        if (baseItem == null)
        {
            return;
        }

        switch (baseItem.ItemTypeEnum)
        {
            case ItemTypeEnum.Weapon:
                EquipWeapon(baseItem as WeaponItem);

                break;
        }
    }

    private void EquipWeapon(WeaponItem weaponItem)
    {
        onEquippedSlotChanged?.Invoke(weaponItem);
    }
}
