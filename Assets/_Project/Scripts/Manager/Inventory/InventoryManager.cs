using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IInventoryManager
{
    [SerializeField] private List<PlayerInventoryData> playerInventoryList = new List<PlayerInventoryData>();
    public event Action<FixedString64Bytes> OnInventoryChanged;

    public static InventoryManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public void ServerAddPlayer(FixedString64Bytes playerID)
    {
        if (GetPlayerInventoryUsingID(playerInventoryList, playerID) == null)
        {
            playerInventoryList.Add(new PlayerInventoryData(playerID));
        }
    }

    public bool ServerAddItem(BaseItem baseItem, FixedString64Bytes playerID)
    {
        PlayerInventoryData playerInventory = GetPlayerInventoryUsingID(playerInventoryList, playerID);

        if (playerInventory == null)
        {
            return false;
        }

        switch (baseItem.ItemTypeEnum)
        {
            case ItemTypeEnum.Weapon:
                WeaponItem weaponItem = baseItem as WeaponItem;

                if (playerInventory.HasWeapon(weaponItem) == true)
                {
                    return false;
                }

                playerInventory.AddWeapon(baseItem as WeaponItem);

                break;
        }

        OnInventoryChanged?.Invoke(playerID);

        return true;
    }

    public void RemoveItemToPlayer(BaseItem baseItem, FixedString64Bytes playerID)
    {
        PlayerInventoryData playerInventory = GetPlayerInventoryUsingID(playerInventoryList, playerID);

        if (playerInventory == null)
        {
            return;
        }

        switch (baseItem.ItemTypeEnum)
        {
            case ItemTypeEnum.Weapon:
                playerInventory.AddWeapon(baseItem as WeaponItem);

                break;
        }

        OnInventoryChanged?.Invoke(playerID);
    }

    public void ResetVariables()
    {
        playerInventoryList.Clear();
    }

    public BaseItem GetPlayerItemBySlot(FixedString64Bytes playerID, int slotIndex)
    {
        PlayerInventoryData playerInventory = GetPlayerInventoryUsingID(playerInventoryList, playerID);

        return playerInventory.GetWeaponBySlot(slotIndex);
    }

    public static PlayerInventoryData GetPlayerInventoryUsingID(List<PlayerInventoryData> playerInventoryList, FixedString64Bytes playerID)
    {
        foreach (PlayerInventoryData playerInventory in playerInventoryList)
        {
            if (playerID == playerInventory.AuthenticateID)
            {
                return playerInventory;
            }
        }

        return null;
    }
}
