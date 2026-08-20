using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkInventoryManager : NetworkBehaviour, IInventoryManager
{
    public static NetworkInventoryManager Instance { get; private set; }

    private NetworkList<NetworkInventoryItem> networkItems;
    private NetworkList<FixedString64Bytes> networkPlayers;

    public event Action<FixedString64Bytes> OnInventoryChanged;

    private void Awake()
    {
        Instance = this;

        networkItems = new NetworkList<NetworkInventoryItem>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        networkPlayers = new NetworkList<FixedString64Bytes>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        networkPlayers.OnListChanged += OnNetworkPlayerListChanged;
        networkItems.OnListChanged += OnNetworkListChanged;

        RebuildLocalInventory();
    }

    public override void OnNetworkDespawn()
    {
        networkItems.OnListChanged -= OnNetworkListChanged;
    }

    private void OnNetworkPlayerListChanged(NetworkListEvent<FixedString64Bytes> change)
    {
        switch (change.Type)
        {
            case NetworkListEvent<FixedString64Bytes>.EventType.Add:
                InventoryManager.Instance.ServerAddPlayer(change.Value);

                break;
        }
    }

    private void OnNetworkListChanged(NetworkListEvent<NetworkInventoryItem> change)
    {
        switch (change.Type)
        {
            case NetworkListEvent<NetworkInventoryItem>.EventType.Add:
                AddItemLocally(change.Value);

                break;

            case NetworkListEvent<NetworkInventoryItem>.EventType.Remove:
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

        NetworkInventoryItem networkItem = new NetworkInventoryItem(playerID, baseItem.ItemID);

        if (networkItems.Contains(networkItem) == true)
        {
            return false;
        }

        networkItems.Add(networkItem);

        return true;
    }

    private void AddItemLocally(NetworkInventoryItem networkItem)
    {
        BaseItem baseItem = ItemDataList.Instance.GetItemById(networkItem.ItemID);

        if (baseItem == null)
        {
            return;
        }

        InventoryManager.Instance.ServerAddItem(baseItem, networkItem.PlayerID);
    }

    private void RemoveItemLocally(NetworkInventoryItem networkItem)
    {
        BaseItem baseItem = ItemDataList.Instance.GetItemById(networkItem.ItemID);

        if (baseItem == null)
        {
            return;
        }

        InventoryManager.Instance.RemoveItemToPlayer(baseItem, networkItem.PlayerID);
    }

    public void RebuildLocalInventory()
    {
        InventoryManager.Instance.ResetVariables();

        foreach (FixedString64Bytes playerID in networkPlayers)
        {
            InventoryManager.Instance.ServerAddPlayer(playerID);
        }

        foreach (NetworkInventoryItem networkItem in networkItems)
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
