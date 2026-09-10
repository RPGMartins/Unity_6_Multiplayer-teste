using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkInventoryManager : NetworkBehaviour, IInventoryManager
{
    public static NetworkInventoryManager Instance { get; private set; }

    [SerializeField] private InventoryManager inventoryManager;

    private NetworkList<InventoryData> networkItems;
    private NetworkList<FixedString64Bytes> networkPlayers;

    public event Action<FixedString64Bytes, WeaponItem> onPlayerEquippedSlotChanged;
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
                AddItemLocally(change.Value);

                break;

            case NetworkListEvent<InventoryData>.EventType.Remove:
                RemoveItemLocally(change.Value);

                break;

            case NetworkListEvent<InventoryData>.EventType.Value:
                UpdateItemLocally(change.Value);

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

    public bool ServerAddItem(BaseItem baseItem, FixedString64Bytes playerID)
    {
        if (IsServer == false)
        {
            return false;
        }

        if (networkPlayers.Contains(playerID) == false)
        {
            return false;
        }

        InventoryData data = new InventoryData(playerID, baseItem.ItemID, false);

        if (networkItems.Contains(data) == true)
        {
            return false;
        }

        networkItems.Add(data);

        return true;
    }

    private void AddItemLocally(InventoryData data)
    {
        BaseItem baseItem = ItemDataList.Instance.GetItemById(data.ItemID);

        if (baseItem == null)
        {
            return;
        }

        inventoryManager.ServerAddItem(baseItem, data.PlayerID);
    }

    private void RemoveItemLocally(InventoryData data)
    {
        BaseItem baseItem = ItemDataList.Instance.GetItemById(data.ItemID);

        if (baseItem == null)
        {
            return;
        }

        inventoryManager.RemoveItemToPlayer(baseItem, data.PlayerID);
    }

    private void UpdateItemLocally(InventoryData changeValue)
    {
        inventoryManager.EquipItem(changeValue);
    }

    public void RebuildLocalInventory()
    {
        inventoryManager.ResetVariables();

        foreach (InventoryData networkItem in networkItems)
        {
            AddItemLocally(networkItem);
        }
    }

    public void EquipItem(InventoryData valueToUpdate)
    {
        for (int i = 0; i < networkItems.Count; i++)
        {
            if (networkItems[i].PlayerID != valueToUpdate.PlayerID)
            {
                continue;
            }

            if (networkItems[i].ItemID != valueToUpdate.ItemID)
            {
                continue;
            }

            networkItems[i] = valueToUpdate;
        }
    }
}
