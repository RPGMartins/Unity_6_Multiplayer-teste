using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IInventoryManager
{
    [SerializeField] private List<InventoryData> playerInventoryList = new List<InventoryData>();
    public event Action<FixedString64Bytes> OnInventoryChanged;

    public static InventoryManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    public bool ServerAddItem(BaseItem baseItem, FixedString64Bytes playerID)
    {
        if (PlayerHasItem(baseItem.ItemID, playerID, out _) == true)
        {
            return false;
        }

        playerInventoryList.Add(new InventoryData(playerID, baseItem.ItemID));
        OnInventoryChanged?.Invoke(playerID);

        return true;
    }

    public void RemoveItemToPlayer(BaseItem baseItem, FixedString64Bytes playerID)
    {
        if (PlayerHasItem(playerID, baseItem.ItemID, out InventoryData inventoryData) == false)
        {
            return;
        }

        playerInventoryList.Remove(inventoryData);
        OnInventoryChanged?.Invoke(playerID);
    }

    public void ResetVariables()
    {
        playerInventoryList.Clear();
    }

    public BaseItem GetPlayerItemBySlot(FixedString64Bytes playerID, int slotIndex)
    {
        string itemID = "";

        switch (slotIndex)
        {
            case 0:
                itemID = "FlashLight";

                break;

            case 1:

                itemID = "Pistol";

                break;

            case 2:
                itemID = "Rifle";

                break;
        }

        if (PlayerHasItem(playerID, itemID, out _) == false)
        {
            return null;
        }

        return ItemDataList.Instance.GetItemById(itemID);
    }

    private bool PlayerHasItem(FixedString64Bytes playerID, FixedString64Bytes itemID, out InventoryData playerInventory)
    {
        playerInventory = default;

        foreach (InventoryData inventoryData in playerInventoryList)
        {
            if (playerID == inventoryData.PlayerID && itemID == inventoryData.ItemID)
            {
                playerInventory = inventoryData;

                return true;
            }
        }

        return false;
    }
}
