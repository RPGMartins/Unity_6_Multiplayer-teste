using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkInventoryManager : NetworkBehaviour, IInventoryManager
{
    public static NetworkInventoryManager Instance { get; private set; }

    private NetworkList<InventoryData> networkItems;
    private NetworkList<FixedString64Bytes> networkPlayers;

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

        InventoryData data = new InventoryData(playerID, baseItem.ItemID);

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

        InventoryManager.Instance.ServerAddItem(baseItem, data.PlayerID);
    }

    private void RemoveItemLocally(InventoryData data)
    {
        BaseItem baseItem = ItemDataList.Instance.GetItemById(data.ItemID);

        if (baseItem == null)
        {
            return;
        }

        InventoryManager.Instance.RemoveItemToPlayer(baseItem, data.PlayerID);
    }

    public void RebuildLocalInventory()
    {
        InventoryManager.Instance.ResetVariables();

        foreach (InventoryData networkItem in networkItems)
        {
            AddItemLocally(networkItem);
        }
    }

    [SerializeField] private bool testeResetInventory;

    private void Update()
    {
        if (testeResetInventory == true)
        {
            testeResetInventory = false;

            RebuildLocalInventory();
        }
    }
}
