using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class InventoryManager : NetworkBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<InventoryData> playerInventoryList = new List<InventoryData>();
    [SerializeField] private List<SerializedInventoryData> playerInventoryListSerialized = new List<SerializedInventoryData>();

    private NetworkList<InventoryData> networkItems;
    private NetworkList<FixedString64Bytes> networkPlayers;

    public event Action<FixedString64Bytes, FixedString64Bytes> onPlayerEquippedSlotChanged;
    public event Action<FixedString64Bytes> OnInventoryChanged;

    private void Awake()
    {
        Instance = this;

        networkItems = new NetworkList<InventoryData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        networkPlayers = new NetworkList<FixedString64Bytes>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        networkItems.OnListChanged += OnNetworkListChanged;

        RebuildLocalInventory();
    }

    public override void OnNetworkDespawn()
    {
        networkItems.OnListChanged -= OnNetworkListChanged;
    }

    private void OnNetworkListChanged(NetworkListEvent<InventoryData> change)
    {
        switch (change.Type)
        {
            case NetworkListEvent<InventoryData>.EventType.Add:
                TryAddItemLocally(change.Value);

                break;

            case NetworkListEvent<InventoryData>.EventType.Value:
                TryEquipItemLocal(change.Value);

                break;
        }
    }

    public void ServerAddPlayer(FixedString64Bytes playerID)
    {
        if (IsServer == false)
        {
            return;
        }

        if (networkPlayers.Contains(playerID) == true)
        {
            return;
        }

        networkPlayers.Add(playerID);
    }

    #region TryAddItem

    public void TryAddItem(BaseItem baseItem, FixedString64Bytes playerID)
    {
        InventoryData data = new InventoryData(playerID, baseItem.ItemID, false);

        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            TryAddItemLocally(data);

            return;
        }

        if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
        {
            return;
        }

        TryAddItemNetwork(data);
    }

    private void TryAddItemNetwork(InventoryData data)
    {
        if (networkPlayers.Contains(data.PlayerID) == false)
        {
            return;
        }

        if (networkItems.Contains(data) == true)
        {
            return;
        }

        networkItems.Add(data);
    }

    private void TryAddItemLocally(InventoryData data)
    {
        BaseItem baseItem = ItemDataList.Instance.GetItemById(data.ItemID);

        if (baseItem == null)
        {
            return;
        }

        FixedString64Bytes playerID = data.PlayerID;

        if (PlayerHasItem(baseItem.ItemID, playerID, out _) == true)    
        {
            return;
        }

        InventoryData inventoryData = new InventoryData(playerID, baseItem.ItemID, false);
        playerInventoryList.Add(inventoryData);
        playerInventoryListSerialized.Add(new SerializedInventoryData(inventoryData));
        OnInventoryChanged?.Invoke(playerID);
    }

    #endregion

    #region TryEquipItem

    public void TryEquipItem(InventoryData data)
    {
        if (NetworkConnectionChecker.CheckConnection() == false)
        {
            TryEquipItemLocal(data);

            return;
        }

        if (NetworkConnectionChecker.CheckIfHasAuthority() == false)
        {
            TryEquipItemNetworkRPC(data);

            return;
        }

        TryEquipItemNetwork(data);
    }

    [Rpc(SendTo.Server)]
    private void TryEquipItemNetworkRPC(InventoryData inventoryData)
    {
        TryEquipItemNetwork(inventoryData);
    }

    private void TryEquipItemNetwork(InventoryData inventoryData)
    {
        for (int i = 0; i < networkItems.Count; i++)
        {
            if (networkItems[i].PlayerID != inventoryData.PlayerID)
            {
                continue;
            }

            if (networkItems[i].ItemID != inventoryData.ItemID)
            {
                if (networkItems[i].Equipped == true)
                {
                    networkItems[i] = new InventoryData(networkItems[i].PlayerID, networkItems[i].ItemID, false);
                }

                continue;
            }

            networkItems[i] = inventoryData;
        }
    }

    private void TryEquipItemLocal(InventoryData inventoryData)
    {
        for (int i = 0; i < playerInventoryList.Count; i++)
        {
            if (playerInventoryList[i].PlayerID != inventoryData.PlayerID)
            {
                continue;
            }

            if (playerInventoryList[i].ItemID != inventoryData.ItemID)
            {
                if (playerInventoryList[i].Equipped == true)
                {
                    InventoryData temp = new InventoryData(playerInventoryList[i].PlayerID, playerInventoryList[i].ItemID, false);

                    playerInventoryList[i] = temp;
                    playerInventoryListSerialized[i] = new SerializedInventoryData(temp);
                }

                continue;
            }

            playerInventoryList[i] = inventoryData;
            playerInventoryListSerialized[i] = new SerializedInventoryData(inventoryData);
            onPlayerEquippedSlotChanged?.Invoke(inventoryData.PlayerID, inventoryData.ItemID);
        }
    }

    #endregion

    #region Utils

    public FixedString64Bytes GetCurrentPlayerEquipedItem(FixedString64Bytes playerID)
    {
        foreach (InventoryData playerInventory in playerInventoryList)
        {
            if (playerInventory.PlayerID == playerID && playerInventory.Equipped == true)
            {
                return playerInventory.ItemID;
            }
        }

        return default;
    }

    private bool PlayerHasItem(FixedString64Bytes playerID, FixedString64Bytes itemID, out InventoryData outInventoryData)
    {
        outInventoryData = default;

        foreach (InventoryData inventoryData in playerInventoryList)
        {
            if (playerID == inventoryData.PlayerID && itemID == inventoryData.ItemID)
            {
                outInventoryData = inventoryData;

                return true;
            }
        }

        return false;
    }

    public bool PlayerCanAddItem(FixedString64Bytes playerID, FixedString64Bytes itemID)
    {
        if (PlayerHasItem(playerID, itemID, out InventoryData _) == true)
        {
            return false;
        }

        return true;
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

    public void RebuildLocalInventory()
    {
        playerInventoryList.Clear();
        playerInventoryListSerialized.Clear();

        foreach (InventoryData networkItem in networkItems)
        {
            TryAddItemLocally(networkItem);
        }
    }

    #endregion
}
