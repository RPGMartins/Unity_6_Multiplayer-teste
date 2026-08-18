using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IInventoryManager
{
    [SerializeField] private List<PlayerInventory> playerInventoryList = new List<PlayerInventory>();
    public event Action<FixedString64Bytes> OnInventoryChanged;

    public static InventoryManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public void AddPlayer(FixedString64Bytes playerID)
    {
        if (GetPlayerUsingIDS(playerInventoryList, playerID) == null)
        {
            playerInventoryList.Add(new PlayerInventory(playerID));
        }
    }

    public bool AddItemToPlayer(BaseItem baseItem, FixedString64Bytes playerID)
    {
        PlayerInventory playerInventory = GetPlayerUsingIDS(playerInventoryList, playerID);

        if (playerInventory == null)
        {
            return false;
        }

        switch (baseItem.ItemTypeEnum)
        {
            case ItemTypeEnum.Weapon:
                playerInventory.AddWeapon(baseItem as WeaponItem);

                break;
        }

        OnInventoryChanged?.Invoke(playerID);

        return true;
    }

    public void RemoveItemToPlayer(BaseItem baseItem, FixedString64Bytes playerID)
    {
        PlayerInventory playerInventory = GetPlayerUsingIDS(playerInventoryList, playerID);

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

    public BaseItem GetPlayerItemBySlot(FixedString64Bytes playerID, int slotIndex)
    {
        PlayerInventory playerInventory = GetPlayerUsingIDS(playerInventoryList, playerID);

        return playerInventory.GetItemBySlot(slotIndex);
    }

    public static PlayerInventory GetPlayerUsingIDS(List<PlayerInventory> playerInventoryList, FixedString64Bytes playerID)
    {
        foreach (PlayerInventory playerInventory in playerInventoryList)
        {
            if (playerID == playerInventory.AuthenticateID)
            {
                return playerInventory;
            }
        }

        return null;
    }
}

[Serializable]
public class PlayerInventory
{
    [SerializeField] private FixedString64Bytes authenticateID;
    [SerializeField] private List<BaseItem> items = new List<BaseItem>();
    [SerializeField] private List<WeaponItem> weapon = new List<WeaponItem>();

    public FixedString64Bytes AuthenticateID => authenticateID;

    public PlayerInventory(FixedString64Bytes authenticateID)
    {
        this.authenticateID = authenticateID;
    }

    public void AddItem(BaseItem baseItem)
    {
        items.Add(baseItem);
    }

    public void RemoveItem(BaseItem baseItem)
    {
        items.Remove(baseItem);
    }

    public void AddWeapon(WeaponItem weaponItem)
    {
        weapon.Add(weaponItem);
    }

    public void RemoveWeapon(WeaponItem weaponItem)
    {
        weapon.Remove(weaponItem);
    }

    public BaseItem GetItemBySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weapon.Count)
        {
            return null;
        }

        return weapon[slotIndex];
    }
}
